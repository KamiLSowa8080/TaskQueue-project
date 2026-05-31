using TaskQueue.Core.Interfaces;

namespace TaskQueue.API.BackgroundServices;

public class StatsBroadcaster : BackgroundService
{
    private static readonly TimeSpan BroadcastInterval = TimeSpan.FromSeconds(3);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IJobNotifier _notifier;
    private readonly ILogger<StatsBroadcaster> _logger;

    public StatsBroadcaster(
        IServiceScopeFactory scopeFactory,
        IJobNotifier notifier,
        ILogger<StatsBroadcaster> logger)
    {
        _scopeFactory = scopeFactory;
        _notifier = notifier;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var statsService = scope.ServiceProvider.GetRequiredService<IQueueStatsService>();
                var stats = await statsService.GetStatsAsync(stoppingToken);

                await _notifier.NotifyStatsUpdatedAsync(stats, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error broadcasting stats");
            }

            await Task.Delay(BroadcastInterval, stoppingToken);
        }
    }
}
