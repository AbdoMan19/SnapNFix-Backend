using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.SnapReport.Commands.CreateSnapReport;

using SnapNFix.Application.Features.SnapReport.DTOs;
using SnapNFix.Application.Features.SnapReport.Queries;
using SnapNFix.Domain.Interfaces;
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
    public async Task<ActionResult<GenericResponseModel<ReportDetailsDto>>> CreateReport(
        [FromForm] CreateSnapReportCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.ErrorList.Count != 0) return BadRequest(result);
        return Ok(result);
    }


    [Authorize("Citizen")]
    [HttpGet("my-reports")]
    public async Task<ActionResult<GenericResponseModel<List<ReportDetailsDto>>>> GetMyReports(
        [FromQuery] GetUserReportsQuery query)
    {
        var result = await _mediator.Send(query);
        if (result.ErrorList.Count != 0) return BadRequest(result);
        return Ok(result);
    }
    
    



    // Admin Operations
    /*[Authorize("Admin")]
    [HttpGet]
    public async Task<ActionResult<GenericResponseModel<PagedList<ReportDetailsDto>>>> GetAllReports(
        [FromQuery] GetReportsQuery query)
    {
        return Ok(await _mediator.Send(query));


    [Authorize("Admin")]
    [HttpGet("statistics")]
    public async Task<ActionResult<GenericResponseModel<ReportStatisticsDto>>> GetReportStatistics(
        [FromQuery] GetReportStatisticsQuery query)
    {
        return Ok(await _mediator.Send(query));
    }*/
}