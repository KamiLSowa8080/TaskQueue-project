using TaskQueue.Core.DTOs;
using TaskQueue.Core.Interfaces;
using TaskQueue.Core.Models;

namespace TaskQueue.Infrastructure.Services;

public class QueueStatsService : IQueueStatsService
{
    private readonly IUnitOfWork _uow;

    public QueueStatsService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<QueueStatsResponse> GetStatsAsync(CancellationToken ct = default)
    {
        var all = await _uow.Jobs.GetAllAsync(ct);

        var total = all.Count;
        var pending = all.Count(j => j.Status == JobStatus.Pending);
        var processing = all.Count(j => j.Status == JobStatus.Processing);
        var completed = all.Count(j => j.Status == JobStatus.Completed);
        var failed = all.Count(j => j.Status == JobStatus.Failed);
        var retrying = all.Count(j => j.Status == JobStatus.Retrying);

        var successRate = total > 0
            ? Math.Round((double)completed / total * 100, 2)
            : 0;

        return new QueueStatsResponse(
            total,
            pending,
            processing,
            completed,
            failed,
            retrying,
            successRate,
            DateTime.UtcNow
        );
    }
}