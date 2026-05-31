using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TaskQueue.Core.Interfaces;

namespace TaskQueue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class MonitoringController : ControllerBase
{
    private readonly IQueueStatsService _statsService;
    private readonly HealthCheckService _healthCheckService;

    public MonitoringController(
        IQueueStatsService statsService,
        HealthCheckService healthCheckService)
    {
        _statsService = statsService;
        _healthCheckService = healthCheckService;
    }

    /// <summary>statystyki kolejki</summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        var stats = await _statsService.GetStatsAsync(ct);
        return Ok(stats);
    }

    /// <summary>health check wszystkich komponentów</summary>
    [HttpGet("health")]
    public async Task<IActionResult> GetHealth(CancellationToken ct)
    {
        var report = await _healthCheckService.CheckHealthAsync(ct);

        var response = new
        {
            Status = report.Status.ToString(),
            Duration = report.TotalDuration.TotalMilliseconds,
            Components = report.Entries.Select(e => new
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description,
                Duration = e.Value.Duration.TotalMilliseconds,
                Tags = e.Value.Tags
            })
        };

        var statusCode = report.Status == HealthStatus.Healthy ? 200 : 503;
        return StatusCode(statusCode, response);
    }
}