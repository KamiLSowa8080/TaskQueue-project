using Microsoft.AspNetCore.SignalR;
using TaskQueue.Core.DTOs;
using TaskQueue.Core.Interfaces;

namespace TaskQueue.API.Hubs;

public class JobNotifier : IJobNotifier
{
    private readonly IHubContext<JobStatusHub> _hub;

    public JobNotifier(IHubContext<JobStatusHub> hub)
    {
        _hub = hub;
    }

    public async Task NotifyJobUpdatedAsync(JobStatusResponse job, CancellationToken ct = default)
    {
        await _hub.Clients
            .Group($"job-{job.Id}")
            .SendAsync("JobUpdated", job, ct);

        await _hub.Clients
            .Group("all")
            .SendAsync("JobListUpdated", job, ct);
    }

    public async Task NotifyStatsUpdatedAsync(QueueStatsResponse stats, CancellationToken ct = default)
    {
        await _hub.Clients
            .Group("all")
            .SendAsync("StatsUpdated", stats, ct);
    }
}
