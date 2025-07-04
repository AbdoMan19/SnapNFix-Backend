using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Issue.DTOs;
using SnapNFix.Application.Features.Issue.Queries;
using SnapNFix.Application.Features.SnapReport.DTOs;
using SnapNFix.Application.Features.FastReport.DTOs; 
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Features.Issue.Queries.GetSnapReportsByIssueId;
using SnapNFix.Application.Interfaces;
using SnapNFix.Infrastructure.Services.UserService;
using SnapNFix.Application.Features.Issue.Commands.UpdateIssue;

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
  public async Task<IActionResult> GetNearbyIssues(
      [FromQuery] GetNearbyIssuesQuery query)
  {
      var result = await _mediator.Send(query);
      if (result.ErrorList.Count != 0) return BadRequest(result);
      return Ok(result);
  }

  [Authorize(Roles = "Citizen,Admin,SuperAdmin")]
  [HttpGet("{id}")]
  public async Task<IActionResult> GetIssueById(Guid id)
  {
    var query = new GetIssueByIdQuery { Id = id };
    var result = await _mediator.Send(query);

    if (result.ErrorList.Count != 0) return BadRequest(result);
    
    return Ok(result);
  }

  [Authorize(Roles = "Citizen,Admin,SuperAdmin")]
  [HttpGet("{id}/snapreports")]
  public async Task<IActionResult> GetSnapReportsByIssueId(Guid id)
  {
    var query = new GetSnapReportsByIssueIdQuery { Id = id };
    var result = await _mediator.Send(query);
    
    if (result.ErrorList.Count != 0) return BadRequest(result);
    
    return Ok(result);
  }

  [Authorize(Roles = "Citizen,Admin,SuperAdmin")]
  [HttpGet("{id}/fastreports")]
  public async Task<IActionResult> GetFastReportsByIssueId(
      Guid id, 
      [FromQuery] int pageNumber = 1,
      [FromQuery] int pageSize = 10)
  {
    var query = new GetFastReportsByIssueIdQuery 
    { 
        Id = id, 
        PageNumber = pageNumber,
        PageSize = pageSize
    };
    var result = await _mediator.Send(query);
    
    if (result.ErrorList.Count != 0) return BadRequest(result);
    
    return Ok(result);
  }

  [Authorize(Roles = "Admin,SuperAdmin")]
  [HttpGet("get-issues")]
  public async Task<IActionResult> GetIssues(
      [FromQuery] GetIssuesQuery query)
  {
    var result = await _mediator.Send(query);
    
    if (result.ErrorList.Count != 0) return BadRequest(result);
    
    return Ok(result);
  }
  [Authorize(Roles = "Admin,SuperAdmin")]
  [HttpPut("{id}")]
    public async Task<IActionResult> UpdateIssue(
        Guid id, 
        [FromBody] UpdateIssueCommand command)
    {
      if (command == null)
        return BadRequest("Invalid request payload or invalid enum value.");
        
        command.Id = id;
        
        var result = await _mediator.Send(command);
        
        if (result.ErrorList.Count != 0) 
            return BadRequest(result);
        
        return Ok(result);
    }
}
