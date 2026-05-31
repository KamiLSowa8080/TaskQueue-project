using Microsoft.Extensions.Logging;
using TaskQueue.Core.DTOs;
using TaskQueue.Core.Interfaces;
using TaskQueue.Core.Messaging;
using TaskQueue.Core.Models;

namespace TaskQueue.Infrastructure.Services;

public class JobService : IJobService
{
    private readonly IUnitOfWork _uow;
    private readonly IQueueProducer _producer;
    private readonly IJobNotifier _notifier;
    private readonly ILogger<JobService> _logger;

    public JobService(
        IUnitOfWork uow,
        IQueueProducer producer,
        IJobNotifier notifier,
        ILogger<JobService> logger)
    {
        _uow = uow;
        _producer = producer;
        _notifier = notifier;
        _logger = logger;
    }

    public async Task<JobStatusResponse> CreateJobAsync(
        CreateJobRequest request,
        CancellationToken ct = default)
    {
        var job = new JobTask
        {
            Type = request.Type,
            Payload = request.Payload,
            Priority = request.Priority,
            MaxRetries = request.MaxRetries,
            Status = JobStatus.Pending
        };

        await _uow.Jobs.AddAsync(job, ct);
        await _uow.SaveChangesAsync(ct);

        // do rmq po zapisie do db
        var message = new JobMessage
        {
            JobId = job.Id,
            Type = job.Type,
            Payload = job.Payload,
            Priority = job.Priority,
            MaxRetries = job.MaxRetries,
            RetryCount = 0
        };

        await _producer.PublishAsync(message, ct);

        _logger.LogInformation(
            "Created and enqueued job {JobId} of type '{Type}' with priority {Priority}",
            job.Id, job.Type, job.Priority);

        var response = MapToResponse(job);
        await _notifier.NotifyJobUpdatedAsync(response, ct);

        return response;
    }

    public async Task<JobStatusResponse?> GetJobAsync(Guid id, CancellationToken ct = default)
    {
        var job = await _uow.Jobs.GetByIdAsync(id, ct);
        return job is null ? null : MapToResponse(job);
    }

    public async Task<IReadOnlyList<JobStatusResponse>> GetAllJobsAsync(CancellationToken ct = default)
    {
        var jobs = await _uow.Jobs.GetAllAsync(ct);
        return jobs.Select(MapToResponse).ToList();
    }

    public async Task<IReadOnlyList<JobStatusResponse>> GetJobsByStatusAsync(
        string status,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<JobStatus>(status, ignoreCase: true, out var jobStatus))
            return [];

        var jobs = await _uow.Jobs.GetByStatusAsync(jobStatus, ct);
        return jobs.Select(MapToResponse).ToList();
    }

    public async Task<bool> CancelJobAsync(Guid id, CancellationToken ct = default)
    {
        var job = await _uow.Jobs.GetByIdAsync(id, ct);

        if (job is null) return false;

        if (job.Status != JobStatus.Pending) return false;

        job.Status = JobStatus.Failed;
        job.ErrorMessage = "Cancelled by user";
        job.CompletedAt = DateTime.UtcNow;

        _uow.Jobs.Update(job);
        await _uow.SaveChangesAsync(ct);

        _logger.LogInformation("Job {JobId} cancelled", id);
        var response = MapToResponse(job);
        await _notifier.NotifyJobUpdatedAsync(response, ct);

        return true;
    }

    private static JobStatusResponse MapToResponse(JobTask job) => new(
        job.Id,
        job.Type,
        job.Status.ToString(),
        job.Priority,
        job.RetryCount,
        job.ErrorMessage,
        job.CreatedAt,
        job.CompletedAt
    );
}
