using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Issue.DTOs;
using SnapNFix.Application.Features.Issue.Queries;
using SnapNFix.Application.Features.SnapReport.DTOs;
using SnapNFix.Application.Features.FastReport.DTOs; // Add this line
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Infrastructure.Services.UserService;

namespace SnapNFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IssueController : ControllerBase
{
  private readonly IMediator _mediator;
  private readonly IUserService UserService;

  public IssueController(IMediator mediator, IUserService userService)
  {
    _mediator = mediator;
    UserService = userService;
  }

  [Authorize("Citizen")]
  [HttpGet("get-nearby-issues")]
  public async Task<ActionResult<GenericResponseModel<List<NearbyIssueDto>>>> GetNearbyIssues(
      [FromQuery] GetNearbyIssuesQuery query)
  {
      var result = await _mediator.Send(query);
      if (result.ErrorList.Count != 0) return BadRequest(result);
      return Ok(result);
  }

  [Authorize(Roles = "Citizen,Admin,SuperAdmin")]
  [HttpGet("{id}")]
  public async Task<ActionResult<GenericResponseModel<IssueDetailsDto>>> GetIssueById(Guid id)
  {
    var query = new GetIssueByIdQuery { Id = id };
    var result = await _mediator.Send(query);

    if (result.ErrorList.Count != 0) return BadRequest(result);
    
    return Ok(result);
  }

  [Authorize(Roles = "Citizen,Admin,SuperAdmin")]
  [HttpGet("{id}/snapreports")]
  public async Task<ActionResult<GenericResponseModel<PagedList<ReportDetailsDto>>>> GetSnapReportsByIssueId(Guid id)
  {
    var query = new GetSnapReportsByIssueIdQuery { Id = id };
    var result = await _mediator.Send(query);
    
    if (result.ErrorList.Count != 0) return BadRequest(result);
    
    return Ok(result);
  }

  [Authorize(Roles = "Citizen,Admin,SuperAdmin")]
  [HttpGet("{id}/fastreports")]
  public async Task<ActionResult<GenericResponseModel<PagedList<FastReportDetailsDto>>>> GetFastReportsByIssueId(
      Guid id, 
      [FromQuery] int pageNumber = 1)
  {
    var query = new GetFastReportsByIssueIdQuery 
    { 
        Id = id, 
        PageNumber = pageNumber,
    };
    var result = await _mediator.Send(query);
    
    if (result.ErrorList.Count != 0) return BadRequest(result);
    
    return Ok(result);
  }

  [Authorize(Roles = "Admin,SuperAdmin")]
  [HttpGet("get-issues")]
  public async Task<ActionResult<GenericResponseModel<PagedList<IssueDetailsDto>>>> GetIssues(
      [FromQuery] GetIssuesQuery query)
  {
    var result = await _mediator.Send(query);
    
    if (result.ErrorList.Count != 0) return BadRequest(result);
    
    return Ok(result);
  }
}
