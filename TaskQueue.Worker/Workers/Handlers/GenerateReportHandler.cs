using Microsoft.Extensions.Logging;
using TaskQueue.Core.Interfaces;
using TaskQueue.Core.Messaging;

namespace TaskQueue.Worker.Handlers;

public class GenerateReportHandler : IJobHandler
{
    public string JobType => "GenerateReport";

    private readonly ILogger<GenerateReportHandler> _logger;

    public GenerateReportHandler(ILogger<GenerateReportHandler> logger)
    {
        _logger = logger;
    }

    public async Task<bool> HandleAsync(JobMessage message, CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Generating report for job {JobId}", message.JobId);

        await Task.Delay(TimeSpan.FromSeconds(3), ct);

        _logger.LogInformation("Report generated for job {JobId}", message.JobId);
        return true;
    }
}