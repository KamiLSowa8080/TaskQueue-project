using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TaskQueue.Core.Interfaces;
using TaskQueue.Core.Messaging;
using TaskQueue.Core.Models;
using TaskQueue.Core.Services;

namespace TaskQueue.Worker.Workers;

public class JobProcessor
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobProcessor> _logger;
    private readonly Dictionary<string, IJobHandler> _handlers;

    public JobProcessor(
        IServiceScopeFactory scopeFactory,
        IEnumerable<IJobHandler> handlers,
        ILogger<JobProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _handlers = handlers.ToDictionary(h => h.JobType, h => h);
    }

    public async Task<bool> ProcessAsync(JobMessage message, CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var producer = scope.ServiceProvider.GetRequiredService<IQueueProducer>();

        // pobierz job z db
        var job = await uow.Jobs.GetByIdAsync(message.JobId, ct);
        if (job is null)
        {
            _logger.LogWarning("Job {JobId} not found in database — skipping", message.JobId);
            return true; // ack a nie retry, bo job nie istnieje
        }

        // oznacz jako w trakcie
        job.Status = JobStatus.Processing;
        job.StartedAt = DateTime.UtcNow;
        uow.Jobs.Update(job);
        await uow.SaveChangesAsync(ct);

        try
        {
            if (!_handlers.TryGetValue(message.Type, out var handler))
            {
                _logger.LogError(
                    "No handler registered for job type '{Type}'", message.Type);

                await MarkFailedAsync(job, uow, $"No handler for type '{message.Type}'", ct);
                return true;
            }

            var success = await handler.HandleAsync(message, ct);

            if (success)
            {
                job.Status = JobStatus.Completed;
                job.CompletedAt = DateTime.UtcNow;
                uow.Jobs.Update(job);
                await uow.SaveChangesAsync(ct);

                _logger.LogInformation(
                    "Job {JobId} completed successfully", message.JobId);
            }
            else
            {
                await HandleRetryOrFailAsync(job, message, uow, producer,
                    "Handler returned false", ct);
            }

            return true;
        }
        catch (OperationCanceledException)
        {
            // shutdown powrot do koleji
            job.Status = JobStatus.Pending;
            uow.Jobs.Update(job);
            await uow.SaveChangesAsync(CancellationToken.None);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception processing job {JobId}", message.JobId);
            await HandleRetryOrFailAsync(job, message, uow, producer, ex.Message, ct);
            return true;
        }
    }

    private async Task HandleRetryOrFailAsync(
        JobTask job,
        JobMessage message,
        IUnitOfWork uow,
        IQueueProducer producer,
        string errorMessage,
        CancellationToken ct)
    {
        job.RetryCount++;
        job.ErrorMessage = errorMessage;

        if (job.RetryCount <= job.MaxRetries)
        {
            var delay = RetryPolicy.GetDelay(job.RetryCount);

            job.Status = JobStatus.Retrying;
            job.NextRetryAt = DateTime.UtcNow + delay;
            uow.Jobs.Update(job);
            await uow.SaveChangesAsync(ct);

            // Wyslij z powrotem do kolejki retry
            var retryMessage = message with
            {
                RetryCount = job.RetryCount,
                EnqueuedAt = DateTime.UtcNow
            };

            await producer.PublishRetryAsync(retryMessage, delay, ct);

            _logger.LogWarning(
                "Job {JobId} failed (attempt {Retry}/{Max}), retrying in {Delay}s",
                job.Id, job.RetryCount, job.MaxRetries, delay.TotalSeconds);
        }
        else
        {
            await MarkFailedAsync(job, uow, errorMessage, ct);

            _logger.LogError(
                "Job {JobId} permanently failed after {Max} retries",
                job.Id, job.MaxRetries);
        }
    }

    private static async Task MarkFailedAsync(
        JobTask job,
        IUnitOfWork uow,
        string errorMessage,
        CancellationToken ct)
    {
        job.Status = JobStatus.Failed;
        job.ErrorMessage = errorMessage;
        job.CompletedAt = DateTime.UtcNow;
        uow.Jobs.Update(job);
        await uow.SaveChangesAsync(ct);
    }
}