using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using PulseQueue.Domain;
using PulseQueue.Infrastructure.Jobs;
using PulseQueue.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PulseQueue.Worker;

public sealed class Worker : BackgroundService
{
    private const int MaxRabbitMqConnectionAttempts = 30;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<Worker> _logger;
    private IConnection? _connection;
    private IModel? _channel;

    public Worker(
        IServiceScopeFactory scopeFactory,
        IOptions<RabbitMqOptions> options,
        ILogger<Worker> logger)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.HostName,
            Port = _options.Port,
            UserName = _options.UserName,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        _connection = await CreateRabbitMqConnectionAsync(factory, stoppingToken);
        _channel = _connection.CreateModel();
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);
        RabbitMqJobQueuePublisher.DeclareQueue(_channel, _options.QueueName);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += ProcessMessageAsync;

        _channel.BasicConsume(
            queue: _options.QueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("Worker is initialized and will consume from RabbitMQ queue {QueueName}", _options.QueueName);

        await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
    }

    private async Task<IConnection> CreateRabbitMqConnectionAsync(
        ConnectionFactory factory,
        CancellationToken stoppingToken)
    {
        for (var attempt = 1; attempt <= MaxRabbitMqConnectionAttempts; attempt++)
        {
            stoppingToken.ThrowIfCancellationRequested();

            try
            {
                return factory.CreateConnection();
            }
            catch (Exception exception) when (attempt < MaxRabbitMqConnectionAttempts)
            {
                _logger.LogWarning(
                    exception,
                    "RabbitMQ connection attempt {Attempt}/{MaxAttempts} failed. Retrying...",
                    attempt,
                    MaxRabbitMqConnectionAttempts);

                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
        }

        return factory.CreateConnection();
    }

    private async Task ProcessMessageAsync(object sender, BasicDeliverEventArgs args)
    {
        if (_channel is null)
        {
            return;
        }

        try
        {
            var json = Encoding.UTF8.GetString(args.Body.ToArray());
            var message = JsonSerializer.Deserialize<JobQueuedMessage>(json);

            if (message is null)
            {
                _logger.LogWarning("Discarding malformed job message with delivery tag {DeliveryTag}", args.DeliveryTag);
                _channel.BasicAck(args.DeliveryTag, multiple: false);
                return;
            }

            await ProcessJobAsync(message);
            _channel.BasicAck(args.DeliveryTag, multiple: false);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Failed to process message with delivery tag {DeliveryTag}", args.DeliveryTag);
            _channel.BasicNack(args.DeliveryTag, multiple: false, requeue: true);
        }
    }

    private async Task ProcessJobAsync(JobQueuedMessage message)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var jobRepository = scope.ServiceProvider.GetRequiredService<IJobRepository>();

        var job = await jobRepository.GetByIdAsync(message.JobId, CancellationToken.None);
        if (job is null)
        {
            _logger.LogWarning("Job {JobId} was not found. Acknowledging message.", message.JobId);
            return;
        }

        if (job.Status is JobStatus.Succeeded or JobStatus.Cancelled)
        {
            _logger.LogInformation("Job {JobId} is already {Status}. Acknowledging message.", job.Id, job.Status);
            return;
        }

        var startedAt = DateTimeOffset.UtcNow;
        await jobRepository.MarkProcessingAsync(job.Id, startedAt, CancellationToken.None);

        _logger.LogInformation("Processing job {JobId} of type {JobType}", job.Id, job.Type);

        await Task.Delay(TimeSpan.FromMilliseconds(500));

        var completedAt = DateTimeOffset.UtcNow;
        await jobRepository.MarkSucceededAsync(job.Id, completedAt, CancellationToken.None);

        _logger.LogInformation("Completed job {JobId}", job.Id);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
}
