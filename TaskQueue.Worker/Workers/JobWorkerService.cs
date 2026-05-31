using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskQueue.Core.Interfaces;
using TaskQueue.Core.Messaging;

namespace TaskQueue.Worker.Workers;

public class JobWorkerService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly JobProcessor _processor;
    private readonly ILogger<JobWorkerService> _logger;

    private readonly string[] _queuesToConsume =
    [
        QueueConstants.HighPriorityQueue,
        QueueConstants.NormalPriorityQueue,
        QueueConstants.LowPriorityQueue
    ];

    public JobWorkerService(
        IServiceScopeFactory scopeFactory,
        JobProcessor processor,
        ILogger<JobWorkerService> logger)
    {
        _scopeFactory = scopeFactory;
        _processor = processor;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JobWorkerService starting...");

        var tasks = _queuesToConsume.Select(queue =>
            ConsumeQueueAsync(queue, stoppingToken));

        await Task.WhenAll(tasks);

        _logger.LogInformation("JobWorkerService stopped.");
    }

    private async Task ConsumeQueueAsync(string queueName, CancellationToken ct)
    {
        _logger.LogInformation("Listening on queue '{Queue}'", queueName);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var consumer = scope.ServiceProvider.GetRequiredService<IQueueConsumer>();

                consumer.StartConsuming(
                    queueName,
                    (message, token) => _processor.ProcessAsync(message, token),
                    ct);

                // scope zywy
                await Task.Delay(Timeout.Infinite, ct);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Consumer for queue '{Queue}' crashed, restarting in 5s...", queueName);

                await Task.Delay(TimeSpan.FromSeconds(5), ct);
            }
        }
    }
}
