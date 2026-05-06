using PulseQueue.Api.Models;

namespace PulseQueue.Api.Services;

public interface IJobService
{
    Task<JobResponse?> GetJobAsync(Guid id, CancellationToken cancellationToken);

    Task<SubmitJobResult> SubmitJobAsync(SubmitJobRequest request, CancellationToken cancellationToken);
}
