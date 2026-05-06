using System.Text.Json;
using PulseQueue.Api.Models;
using PulseQueue.Domain;
using PulseQueue.Infrastructure.Jobs;
using PulseQueue.Infrastructure.Messaging;

namespace PulseQueue.Api.Services;

public sealed class JobService : IJobService
{
    private readonly IJobRepository _jobRepository;
    private readonly IJobQueuePublisher _jobPublisher;

    public JobService(IJobRepository jobRepository, IJobQueuePublisher publisher)
    {
        _jobRepository = jobRepository;
        _jobPublisher = publisher;
    }

    public async Task<JobResponse?> GetJobAsync(Guid id, CancellationToken cancellationToken)
    {
        var job = await _jobRepository.GetByIdAsync(id, cancellationToken);

        return job is null ? null : JobResponse.FromJob(job);
    }

    public async Task<SubmitJobResult> SubmitJobAsync(
        SubmitJobRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Type))
        {
            return SubmitJobResult.Failure("Job type is required.");
        }

        if (request.Payload.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null)
        {
            return SubmitJobResult.Failure("Job payload is required.");
        }

        var now = DateTimeOffset.UtcNow;
        var job = new Job
        {
            Id = Guid.NewGuid(),
            Type = request.Type.Trim(),
            Payload = request.Payload.GetRawText(),
            Status = JobStatus.Queued,
            CreatedAt = now,
            UpdatedAt = now
        };

        await _jobRepository.CreateAsync(job, cancellationToken);

        await _jobPublisher.PublishAsync(new JobQueuedMessage(job.Id, job.Type), cancellationToken);

        return SubmitJobResult.Success(JobResponse.FromJob(job));
    }
}
