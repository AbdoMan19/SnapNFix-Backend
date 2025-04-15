using MediatR;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.LoginWithPhoneOrEmail;
using SnapNFix.Application.Features.Auth.Logout;
using SnapNFix.Application.Features.Auth.RefreshToken;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Features.Users.Commands.RegisterUser;

namespace SnapNFix.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginWithPhoneOrEmailCommand command)
    {
        command.IpAddress = GetIpAddress();
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }


    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        command.IpAddress = GetIpAddress();
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutCommand command)
    {
        command.IpAddress = GetIpAddress();
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register(RegisterUserCommand command)
    {
        
        if(command == null)
            return BadRequest("Invalid request");
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];

        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";
    }
}