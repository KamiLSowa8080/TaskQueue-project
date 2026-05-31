using TaskQueue.Core.DTOs;
using TaskQueue.Core.Interfaces;

namespace TaskQueue.Infrastructure.Services;

public class NoOpJobNotifier : IJobNotifier
{
    public Task NotifyJobUpdatedAsync(JobStatusResponse job, CancellationToken ct = default)
        => Task.CompletedTask;

    public Task NotifyStatsUpdatedAsync(QueueStatsResponse stats, CancellationToken ct = default)
        => Task.CompletedTask;
}
