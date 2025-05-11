using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.SnapReport.Commands.CreateSnapReport;

using SnapNFix.Application.Features.SnapReport.DTOs;

using SnapNFix.Domain.Interfaces;


namespace SnapNFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SnapReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SnapReportsController(IMediator mediator, IUserService userService)
    {
        _mediator = mediator;
    }

    // User Operations
    [Authorize("Citizen")]
    [HttpPost("create")]
    public async Task<ActionResult<GenericResponseModel<ReportDetailsDto>>> CreateReport(
        [FromForm] CreateSnapReportCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.ErrorList.Count != 0) return BadRequest(result);
        return Ok(result);
    }

    /*[Authorize("Citizen")]
    [HttpGet("my-reports")]
    public async Task<ActionResult<GenericResponseModel<List<ReportDetailsDto>>>> GetMyReports(
        [FromQuery] GetUserReportsQuery query)
    {
        query.UserId = (await _userService.GetCurrentUserAsync()).Id;
        return Ok(await _mediator.Send(query));
    }*/

    /*[Authorize("Citizen")]
    [HttpPut("{id}/comment")]
    public async Task<ActionResult<GenericResponseModel<ReportDetailsDto>>> UpdateReportComment(
        Guid id, [FromBody] UpdateReportCommentCommand command)
    {
        command.ReportId = id;
        command.UserId = (await _userService.GetCurrentUserAsync()).Id;
        return Ok(await _mediator.Send(command));
    }*/

    /*[Authorize("Citizen")]
    [HttpDelete("{id}")]
    public async Task<ActionResult<GenericResponseModel<bool>>> DeleteReport(Guid id)
    {
        var command = new DeleteReportCommand 
        { 
            ReportId = id,
            UserId = (await _userService.GetCurrentUserAsync()).Id
        };
        return Ok(await _mediator.Send(command));
    }*/

    // Admin Operations
    /*[Authorize("Admin")]
    [HttpGet]
    public async Task<ActionResult<GenericResponseModel<PagedList<ReportDetailsDto>>>> GetAllReports(
        [FromQuery] GetReportsQuery query)
    {
        return Ok(await _mediator.Send(query));
    }

    [Authorize("Admin")]
    [HttpPut("{id}/status")]
    public async Task<ActionResult<GenericResponseModel<ReportDetailsDto>>> UpdateReportStatus(
        Guid id, [FromBody] UpdateReportStatusCommand command)
    {
        command.ReportId = id;
        return Ok(await _mediator.Send(command));
    }

    [Authorize("Admin")]
    [HttpGet("statistics")]
    public async Task<ActionResult<GenericResponseModel<ReportStatisticsDto>>> GetReportStatistics(
        [FromQuery] GetReportStatisticsQuery query)
    {
        return Ok(await _mediator.Send(query));
    }*/
}