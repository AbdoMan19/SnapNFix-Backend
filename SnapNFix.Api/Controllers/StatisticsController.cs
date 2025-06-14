using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize("Admin")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsService _StatisticsService;
    private readonly ILogger<StatisticsController> _logger;

    public StatisticsController(
        IStatisticsService StatisticsService,
        ILogger<StatisticsController> logger)
    {
        _StatisticsService = StatisticsService;
        _logger = logger;
    }

    [HttpGet("statistics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StatisticsDto>> GetStatistics(CancellationToken cancellationToken)
    {
        try
        {
            var statistics = await _StatisticsService.GetStatisticsAsync(cancellationToken);
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving Statistics statistics");
            return StatusCode(500, "An error occurred while retrieving Statistics statistics");
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
            var trends = await _StatisticsService.GetIncidentTrendsAsync(interval, cancellationToken);
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
            var geoData = await _StatisticsService.GetGeographicDistributionAsync(limit, cancellationToken);
            return Ok(geoData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving geographic distribution");
            return StatusCode(500, "An error occurred while retrieving geographic distribution");
        }
    }
}