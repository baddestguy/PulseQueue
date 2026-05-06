using PulseQueue.Api.Models;

namespace PulseQueue.Api.Services;

public sealed record SubmitJobResult(JobResponse? Job, string? Error)
{
    public bool Succeeded => Job is not null;

    public static SubmitJobResult Success(JobResponse job) => new(job, Error: null);

    public static SubmitJobResult Failure(string error) => new(Job: null, error);
}
