using System.Text.Json;
using PulseQueue.Domain;

namespace PulseQueue.Api.Models;

public sealed record JobResponse(
    Guid Id,
    string Type,
    JsonElement Payload,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? StartedAt,
    DateTimeOffset? CompletedAt,
    string? FailureReason)
{
    public static JobResponse FromJob(Job job) =>
        new(
            job.Id,
            job.Type,
            JsonSerializer.Deserialize<JsonElement>(job.Payload),
            job.Status.ToString(),
            job.CreatedAt,
            job.UpdatedAt,
            job.StartedAt,
            job.CompletedAt,
            job.FailureReason);
}
