using System.Text.Json;

namespace PulseQueue.Api.Models;

public sealed record SubmitJobRequest(string Type, JsonElement Payload);
