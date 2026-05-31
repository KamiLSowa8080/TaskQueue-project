namespace TaskQueue.Core.DTOs;

public record QueueStatsResponse(
    int TotalJobs,
    int PendingJobs,
    int ProcessingJobs,
    int CompletedJobs,
    int FailedJobs,
    int RetryingJobs,
    double SuccessRatePercent,
    DateTime GeneratedAt
);
