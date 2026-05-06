namespace PulseQueue.Domain;

public enum JobStatus
{
    Queued = 0,
    Processing = 1,
    Succeeded = 2,
    Failed = 3,
    DeadLettered = 4,
    Cancelled = 5
}
