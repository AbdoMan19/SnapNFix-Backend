using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Admin.Commands.AdminLogin;
using SnapNFix.Application.Features.Admin.Commands.DeleteUser;
using SnapNFix.Application.Features.Admin.Commands.RegisterAdmin;
using SnapNFix.Application.Features.Admin.Commands.SuspendUser;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Resources;

namespace SnapNFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IMediator mediator, ILogger<AdminController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GenericResponseModel<LoginResponse>>> AdminLogin([FromBody] AdminLoginCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpPost("register")]
    [Authorize("SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GenericResponseModel<LoginResponse>>> RegisterAdmin([FromBody] RegisterAdminCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpGet("users")]
    [Authorize("SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GenericResponseModel<PagedList<UserDetailsDto>>>> GetAllUsers(
        [FromQuery] GetAllUsersQuery query)
    {
        var result = await _mediator.Send(query);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpPut("users/{userId}/suspend")]
    [Authorize("SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenericResponseModel<bool>>> SuspendUser(
        [FromRoute] Guid userId,
        [FromBody] SuspendUserRequest request)
    {
        _logger.LogInformation("Suspend user request received for UserId: {UserId}, IsSuspended: {IsSuspended}",
            userId, request.IsSuspended);

        if (userId == Guid.Empty)
        {
            _logger.LogWarning("Invalid user ID received: {UserId}", userId);
            return BadRequest(GenericResponseModel<bool>.Failure("Invalid user ID"));
        }

        var command = new SuspendUserQuery
        {
            UserId = userId,
            IsSuspended = request.IsSuspended
        };

        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpDelete("users/{userId}")]
    [Authorize("SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GenericResponseModel<bool>>> DeleteUser([FromRoute] Guid userId)
    {
        _logger.LogInformation("Delete user request received for UserId: {UserId}", userId);

        if (userId == Guid.Empty)
        {
            _logger.LogWarning("Invalid user ID received: {UserId}", userId);
            return BadRequest(GenericResponseModel<bool>.Failure(Shared.UserNotFound));
        }

        var command = new DeleteUserQuery { UserId = userId };
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }
    
    [HttpPut("profile")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<GenericResponseModel<bool>>> UpdateProfile([FromBody] UpdateAdminProfileCommand command)
    {
        _logger.LogInformation("Admin profile update request received for user {UserId}", 
            User.FindFirstValue(ClaimTypes.NameIdentifier));

        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

}

