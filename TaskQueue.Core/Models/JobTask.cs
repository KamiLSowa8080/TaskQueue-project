namespace TaskQueue.Core.Models;

public class JobTask
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Type { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;   // JSON z danymi zadania

    public JobStatus Status { get; set; } = JobStatus.Pending;

    public int Priority { get; set; } = 0;    // wyższy = ważniejszy
    public int RetryCount { get; set; } = 0;
    public int MaxRetries { get; set; } = 3;

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? NextRetryAt { get; set; }
}