using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Statistics.Queries.GetDashboardSummary;
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

    [HttpGet("dashboard-summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GenericResponseModel<StatisticsDto>>> GetDashboardSummary(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetDashboardSummaryQuery(), cancellationToken);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpGet("metrics")]
    public async Task<ActionResult<GenericResponseModel<MetricsOverviewDto>>> GetMetrics(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMetricsQuery(), cancellationToken);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GenericResponseModel<List<CategoryDistributionDto>>>> GetCategoryDistribution(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetCategoryDistributionQuery(), cancellationToken);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpGet("monthly-target")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GenericResponseModel<MonthlyTargetDto>>> GetMonthlyTarget(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetMonthlyTargetQuery(), cancellationToken);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpGet("trends")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<GenericResponseModel<List<IncidentTrendDto>>>> GetIncidentTrends(
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
    public async Task<ActionResult<GenericResponseModel<List<GeographicDistributionDto>>>> GetGeographicDistribution(
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
    public async Task<ActionResult<GenericResponseModel<StatisticsDto>>> GetStatistics(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetStatisticsQuery(), cancellationToken);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }
}