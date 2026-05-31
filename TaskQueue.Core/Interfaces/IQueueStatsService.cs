using TaskQueue.Core.DTOs;

namespace TaskQueue.Core.Interfaces;

public interface IQueueStatsService
{
    Task<QueueStatsResponse> GetStatsAsync(CancellationToken ct = default);
}
