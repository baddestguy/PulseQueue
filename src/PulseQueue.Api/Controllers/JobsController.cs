using Microsoft.AspNetCore.Mvc;
using PulseQueue.Api.Models;
using PulseQueue.Api.Services;

namespace PulseQueue.Api.Controllers;

[ApiController]
[Route("jobs")]
public sealed class JobsController : ControllerBase
{
    private readonly IJobService JobService;

    public JobsController(IJobService jobService)
    {
        JobService = jobService;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitJob(
        SubmitJobRequest request,
        CancellationToken cancellationToken)
    {
        var result = await JobService.SubmitJobAsync(request, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new ErrorResponse(result.Error!));
        }

        return Accepted($"/jobs/{result.Job!.Id}", result.Job);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetJob(Guid id, CancellationToken cancellationToken)
    {
        var job = await JobService.GetJobAsync(id, cancellationToken);

        return job is null ? NotFound() : Ok(job);
    }
}
