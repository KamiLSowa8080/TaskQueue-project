namespace TaskQueue.Core.DTOs;

public record JobStatusResponse(
    Guid Id,
    string Type,
    string Status,
    int Priority,
    int RetryCount,
    string? ErrorMessage,
    DateTime CreatedAt,
    DateTime? CompletedAt
);