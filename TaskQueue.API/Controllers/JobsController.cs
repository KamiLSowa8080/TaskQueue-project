using Microsoft.AspNetCore.Mvc;
using TaskQueue.Core.DTOs;
using TaskQueue.Core.Interfaces;

namespace TaskQueue.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class JobsController : ControllerBase
{
    private readonly IJobService _jobService;
    private readonly ILogger<JobsController> _logger;

    public JobsController(IJobService jobService, ILogger<JobsController> logger)
    {
        _jobService = jobService;
        _logger = logger;
    }

    /// <summary>tworzy nowe zadanie i dodaje je do kolejki</summary>
    [HttpPost]
    [ProducesResponseType(typeof(JobStatusResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateJob(
        [FromBody] CreateJobRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Type))
            return BadRequest("Job type is required.");

        var result = await _jobService.CreateJobAsync(request, ct);
        return CreatedAtAction(nameof(GetJob), new { id = result.Id }, result);
    }

    /// <summary>pobiera status konkretnego zadania</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JobStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJob(Guid id, CancellationToken ct)
    {
        var result = await _jobService.GetJobAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>pobiera wszystkie zadania</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<JobStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllJobs(CancellationToken ct)
    {
        var results = await _jobService.GetAllJobsAsync(ct);
        return Ok(results);
    }

    /// <summary>filtruje zadania po statusie</summary>
    [HttpGet("by-status/{status}")]
    [ProducesResponseType(typeof(IReadOnlyList<JobStatusResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByStatus(string status, CancellationToken ct)
    {
        var results = await _jobService.GetJobsByStatusAsync(status, ct);
        return Ok(results);
    }

    /// <summary>anuluje zadanie 9tylko pending)</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CancelJob(Guid id, CancellationToken ct)
    {
        var result = await _jobService.CancelJobAsync(id, ct);

        if (!result)
        {
            var job = await _jobService.GetJobAsync(id, ct);
            if (job is null) return NotFound();
            return Conflict("Only Pending jobs can be cancelled.");
        }

        return NoContent();
    }
}