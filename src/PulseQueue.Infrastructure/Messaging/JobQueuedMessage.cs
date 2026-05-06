namespace PulseQueue.Infrastructure.Messaging;

public sealed record JobQueuedMessage(Guid JobId, string Type);
