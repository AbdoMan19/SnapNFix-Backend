using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Issue.DTOs;
using SnapNFix.Application.Features.Issue.Queries;
using SnapNFix.Application.Features.SnapReport.Commands.CreateSnapReport;
using SnapNFix.Application.Features.SnapReport.DTOs;
using SnapNFix.Application.Features.SnapReport.Queries;
using SnapNFix.Domain.Interfaces;
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
  public async Task<ActionResult<GenericResponseModel<List<IssueDetailsDto>>>> GetNearbyIssues(
      [FromQuery] GetNearbyIssuesQuery query)
  {
    return Ok(await _mediator.Send(query));
  }

  [Authorize("Citizen")]
  [HttpGet("get-user-issues")]
  public async Task<ActionResult<GenericResponseModel<PagedList<IssueDetailsDto>>>> GetUserIssues(
      [FromQuery] GetUserIssuesQuery query)
  {
    return Ok(await _mediator.Send(query));
  }
}