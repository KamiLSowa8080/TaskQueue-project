using TaskQueue.Core.DTOs;

namespace TaskQueue.Core.Interfaces;

public interface IJobService
{
    Task<JobStatusResponse> CreateJobAsync(CreateJobRequest request, CancellationToken ct = default);
    Task<JobStatusResponse?> GetJobAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<JobStatusResponse>> GetAllJobsAsync(CancellationToken ct = default);
    Task<IReadOnlyList<JobStatusResponse>> GetJobsByStatusAsync(string status, CancellationToken ct = default);
    Task<bool> CancelJobAsync(Guid id, CancellationToken ct = default);
}