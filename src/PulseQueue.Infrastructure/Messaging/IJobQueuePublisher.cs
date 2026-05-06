namespace PulseQueue.Infrastructure.Messaging;

public interface IJobQueuePublisher
{
    Task PublishAsync(JobQueuedMessage message, CancellationToken cancellationToken);
}
