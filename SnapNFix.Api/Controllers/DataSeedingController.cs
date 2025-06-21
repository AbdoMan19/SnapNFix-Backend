using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Infrastructure.Context;

namespace SnapNFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DataSeedingController : ControllerBase
{
    private readonly IDataSeedingService _seedingService;
    private readonly ILogger<DataSeedingController> _logger;

    public DataSeedingController(IDataSeedingService seedingService, ILogger<DataSeedingController> logger)
    {
        _seedingService = seedingService;
        _logger = logger;
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedData([FromQuery] int users = 100, [FromQuery] int reports = 1000)
    {
        try
        {
            _logger.LogInformation("Starting data seeding with {Users} users and {Reports} reports", users, reports);
            
            await _seedingService.SeedLargeDatasetAsync(users, reports);
            
            return Ok(new { 
                Message = "Data seeding completed successfully", 
                UsersCreated = users, 
                ReportsCreated = reports 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data seeding");
            return StatusCode(500, new { Message = "Data seeding failed", Error = ex.Message });
        }
    }


    [HttpPost("seed-large")]
    public async Task<IActionResult> SeedLargeData()
    {
        try
        {
            _logger.LogInformation("Starting large data seeding");
            
            await _seedingService.SeedLargeDatasetAsync(numberOfUsers: 20, numberOfReports: 120);
            
            return Ok(new { 
                Message = "Large data seeding completed successfully", 
                UsersCreated = 20, 
                ReportsCreated = 120 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during large data seeding");
            return StatusCode(500, new { Message = "Large data seeding failed", Error = ex.Message });
        }
    }

    [HttpDelete("clear-all")]
    public async Task<IActionResult> ClearAllData([FromQuery] bool confirm = false)
    {
        if (!confirm)
        {
            return BadRequest("You must set confirm=true to clear all data");
        }

        try
        {
            _logger.LogWarning("Clearing all data from database");
            
            await _seedingService.ClearAllDataAsync();
            
            return Ok(new { Message = "All data cleared successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing data");
            return StatusCode(500, new { Message = "Data clearing failed", Error = ex.Message });
        }
    }
}
