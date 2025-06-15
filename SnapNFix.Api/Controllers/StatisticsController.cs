using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize("Admin")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _statisticsService;
    private readonly ILogger<StatisticsController> _logger;

    public StatisticsController(
        IStatisticsService statisticsService,
        ILogger<StatisticsController> logger)
    {
        _statisticsService = statisticsService;
        _logger = logger;
    }

    [HttpGet("dashboard-summary")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StatisticsDto>> GetDashboardSummary(CancellationToken cancellationToken)
    {
        try
        {
            var statistics = await _statisticsService.GetDashboardSummaryAsync(cancellationToken);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard summary");
            return StatusCode(500, "An error occurred while retrieving dashboard summary");
        }
    }

    [HttpGet("metrics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MetricsOverviewDto>> GetMetrics(CancellationToken cancellationToken)
    {
        try
        {
            var metrics = await _statisticsService.GetMetricsAsync(cancellationToken);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving metrics");
            return StatusCode(500, "An error occurred while retrieving metrics");
        }
    }

    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<CategoryDistributionDto>>> GetCategoryDistribution(CancellationToken cancellationToken)
    {
        try
        {
            var categories = await _statisticsService.GetCategoryDistributionAsync(cancellationToken);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving category distribution");
            return StatusCode(500, "An error occurred while retrieving category distribution");
        }
    }

    [HttpGet("monthly-target")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<MonthlyTargetDto>> GetMonthlyTarget(CancellationToken cancellationToken)
    {
        try
        {
            var target = await _statisticsService.GetMonthlyTargetAsync(cancellationToken);
            return Ok(target);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving monthly target");
            return StatusCode(500, "An error occurred while retrieving monthly target");
        }
    }

    [HttpGet("trends")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<IncidentTrendDto>>> GetIncidentTrends(
        [FromQuery] string interval = "monthly", 
        CancellationToken cancellationToken = default)
    {
        if (!new[] { "monthly", "quarterly", "yearly" }.Contains(interval.ToLower()))
        {
            return BadRequest("Invalid interval. Use 'monthly', 'quarterly', or 'yearly'");
        }

        try
        {
            var trends = await _statisticsService.GetIncidentTrendsAsync(interval, cancellationToken);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving incident trends for interval {Interval}", interval);
            return StatusCode(500, "An error occurred while retrieving incident trends");
        }
    }

    [HttpGet("geographic-distribution")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<GeographicDistributionDto>>> GetGeographicDistribution(
        [FromQuery] int limit = 10, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var geoData = await _statisticsService.GetGeographicDistributionAsync(limit, cancellationToken);
            return Ok(geoData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving geographic distribution");
            return StatusCode(500, "An error occurred while retrieving geographic distribution");
        }
    }

    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StatisticsDto>> GetStatistics(CancellationToken cancellationToken)
    {
        try
        {
            var statistics = await _statisticsService.GetStatisticsAsync(cancellationToken);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Statistics statistics");
            return StatusCode(500, "An error occurred while retrieving Statistics statistics");
        }
    }
}