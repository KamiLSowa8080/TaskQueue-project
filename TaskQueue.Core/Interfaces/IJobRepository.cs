using TaskQueue.Core.Models;

namespace TaskQueue.Core.Interfaces;

public interface IJobRepository : IRepository<JobTask>
{
    // zapytania specyficzne dla zadań
    Task<IReadOnlyList<JobTask>> GetByStatusAsync(JobStatus status, CancellationToken ct = default);
    Task<IReadOnlyList<JobTask>> GetPendingJobsOrderedByPriorityAsync(int batchSize = 10, CancellationToken ct = default);
    Task<IReadOnlyList<JobTask>> GetJobsReadyForRetryAsync(CancellationToken ct = default);
}