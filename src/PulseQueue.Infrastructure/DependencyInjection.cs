using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PulseQueue.Infrastructure.Data;
using PulseQueue.Infrastructure.Jobs;
using PulseQueue.Infrastructure.Messaging;

namespace PulseQueue.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPulseQueueInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IDbConnectionFactory, PostgresConnectionFactory>();
        services.AddScoped<IJobRepository, JobRepository>();

        services.Configure<RabbitMqOptions>(options =>
        {
            var section = configuration.GetSection(RabbitMqOptions.SectionName);

            options.HostName = section["HostName"] ?? options.HostName;
            options.UserName = section["UserName"] ?? options.UserName;
            options.Password = section["Password"] ?? options.Password;
            options.QueueName = section["QueueName"] ?? options.QueueName;

            if (int.TryParse(section["Port"], out var port))
            {
                options.Port = port;
            }
        });
        services.AddSingleton<IJobQueuePublisher, RabbitMqJobQueuePublisher>();

        return services;
    }
}
