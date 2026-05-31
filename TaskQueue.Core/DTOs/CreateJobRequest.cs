namespace TaskQueue.Core.DTOs;

public record CreateJobRequest(
    string Type,
    string Payload,
    int Priority = 0,
    int MaxRetries = 3
);