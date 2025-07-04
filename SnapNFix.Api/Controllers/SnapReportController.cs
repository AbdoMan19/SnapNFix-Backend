using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.SnapReport.Commands.CreateSnapReport;
using SnapNFix.Application.Features.SnapReport.DTOs;
using SnapNFix.Application.Features.SnapReport.Queries;
using SnapNFix.Application.Features.SnapReport.Queries.GetUserReportsStatistics;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Interfaces;
using SnapNFix.Infrastructure.Services.UserService;


namespace SnapNFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SnapReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IUserService UserService;

    public SnapReportsController(IMediator mediator, IUserService userService)
    {
        _mediator = mediator;
        UserService = userService;
    }

    [Authorize("Citizen")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateReport(
        [FromForm] CreateSnapReportCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.ErrorList.Count != 0) return BadRequest(result);
        return Ok(result);
    }

    [Authorize("Citizen")]
    [HttpGet("my-reports")]
    public async Task<IActionResult> GetMyReports(
        [FromQuery] GetUserReportsQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.ErrorList.Count != 0) return BadRequest(result);
        return Ok(result);
    }
    
    [Authorize("Citizen")]
    [HttpGet("statistics")]
    public async Task<IActionResult> Statistics()
    {
        var result = await _mediator.Send(new GetUserReportsStatisticsQuery());
        if (result.ErrorList.Count != 0) return BadRequest(result);
        return Ok(result);
    }

    
}
