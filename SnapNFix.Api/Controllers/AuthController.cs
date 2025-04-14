using MediatR;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.LoginWithPhoneOrEmail;
using SnapNFix.Application.Features.Auth.Logout;
using SnapNFix.Application.Features.Auth.RefreshToken;
using SnapNFix.Application.Features.Auth.Dtos;

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
    public async Task<GenericResponseModel<AuthResponse>> Login([FromBody] LoginWithPhoneOrEmailCommand command)
    {
        command.IpAddress = GetIpAddress();
        var result = await _mediator.Send(command);
        return result;
    }


    [HttpPost("refresh-token")]
    public async Task<GenericResponseModel<AuthResponse>> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        command.IpAddress = GetIpAddress();
        var result = await _mediator.Send(command);
        return result;
    }

    [HttpPost("logout")]
    public async Task<GenericResponseModel<bool>> Logout([FromBody] LogoutCommand command)
    {
        command.IpAddress = GetIpAddress();
        var result = await _mediator.Send(command);
        return result;
    }

    private string GetIpAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];

        return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "Unknown";
    }
}