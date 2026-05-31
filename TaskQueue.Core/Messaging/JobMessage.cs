namespace TaskQueue.Core.Messaging;

public record JobMessage
{
    public Guid JobId { get; init; }
    public string Type { get; init; } = string.Empty;
    public string Payload { get; init; } = string.Empty;
    public int Priority { get; init; }
    public int RetryCount { get; init; }
    public int MaxRetries { get; init; }
    public DateTime EnqueuedAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; } // do śledzenia między serwisami
}