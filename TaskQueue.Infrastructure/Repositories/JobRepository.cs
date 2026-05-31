using Microsoft.EntityFrameworkCore;
using TaskQueue.Core.Interfaces;
using TaskQueue.Core.Models;
using TaskQueue.Infrastructure.Data;

namespace TaskQueue.Infrastructure.Repositories;

public class JobRepository : BaseRepository<JobTask>, IJobRepository
{
    public JobRepository(AppDbContext context) : base(context) { }

    public async Task<IReadOnlyList<JobTask>> GetByStatusAsync(
        JobStatus status,
        CancellationToken ct = default)
        => await _dbSet
            .Where(j => j.Status == status)
            .OrderByDescending(j => j.Priority)
            .ThenBy(j => j.CreatedAt)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<JobTask>> GetPendingJobsOrderedByPriorityAsync(
        int batchSize = 10,
        CancellationToken ct = default)
        => await _dbSet
            .Where(j => j.Status == JobStatus.Pending)
            .OrderByDescending(j => j.Priority) // wyzszy priorytet = pierwszy
            .ThenBy(j => j.CreatedAt)           // przy rownym = starszy wygrywa
            .Take(batchSize)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<JobTask>> GetJobsReadyForRetryAsync(
        CancellationToken ct = default)
        => await _dbSet
            .Where(j =>
                j.Status == JobStatus.Retrying &&
                j.NextRetryAt != null &&
                j.NextRetryAt <= DateTime.UtcNow)
            .OrderByDescending(j => j.Priority)
            .ToListAsync(ct);
}