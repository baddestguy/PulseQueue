using PulseQueue.Domain;

namespace PulseQueue.Infrastructure.Jobs;

public interface IJobRepository
{
    Task CreateAsync(Job job, CancellationToken cancellationToken);

    Task<Job?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task MarkProcessingAsync(Guid id, DateTimeOffset startedAt, CancellationToken cancellationToken);

    Task MarkSucceededAsync(Guid id, DateTimeOffset completedAt, CancellationToken cancellationToken);
}
