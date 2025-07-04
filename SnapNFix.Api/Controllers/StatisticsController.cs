using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Application.Features.Statistics.Queries.GetMetrics;
using SnapNFix.Application.Features.Statistics.Queries.GetCategoryDistribution;
using SnapNFix.Application.Features.Statistics.Queries.GetMonthlyTarget;
using SnapNFix.Application.Features.Statistics.Queries.GetIncidentTrends;
using SnapNFix.Application.Features.Statistics.Queries.GetGeographicDistribution;
using SnapNFix.Application.Features.Statistics.Queries.GetStatistics;

namespace SnapNFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,SuperAdmin")]
public class StatisticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public StatisticsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    

    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMetricsQuery(), cancellationToken);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCategoryDistribution(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoryDistributionQuery(), cancellationToken);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpGet("monthly-target")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMonthlyTarget(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMonthlyTargetQuery(), cancellationToken);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpGet("trends")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetIncidentTrends(
        [FromQuery] GetIncidentTrendsQuery query, 
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpGet("geographic-distribution")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetGeographicDistribution(
        [FromQuery] GetGeographicDistributionQuery query, 
        CancellationToken cancellationToken = default)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStatisticsQuery(), cancellationToken);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }
}