using TaskQueue.Core.DTOs;

namespace TaskQueue.Core.Interfaces;

public interface IJobNotifier
{
    Task NotifyJobUpdatedAsync(JobStatusResponse job, CancellationToken ct = default);
    Task NotifyStatsUpdatedAsync(QueueStatsResponse stats, CancellationToken ct = default);
}
