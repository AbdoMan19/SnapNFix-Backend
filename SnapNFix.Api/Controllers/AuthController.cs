using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SnapNFix.Application.Features.Auth.ForgetPassword.RequestForgetPasswordOtp;
using SnapNFix.Application.Features.Auth.LoginWithPhoneOrEmail;
using SnapNFix.Application.Features.Auth.Logout;
using SnapNFix.Application.Features.Auth.RefreshToken;
using SnapNFix.Application.Features.Users.Commands.RegisterUser;
using SnapNFix.Application.Features.Auth.ForgetPassword.VerifyForgetPasswordOtp;
using SnapNFix.Application.Features.Auth.ResetPassword;
using SnapNFix.Application.Features.Auth.PhoneVerification.RequestPhoneVerificationOtp;
using SnapNFix.Application.Features.Auth.PhoneVerification.VerifyPhoneVerificationOtp;
using SnapNFix.Application.Features.Auth.ForgetPassword.ResendForgetPasswordOtp;
using SnapNFix.Application.Features.Auth.PhoneVerification.ResendPhoneVerificationOtp;
using SnapNFix.Application.Features.Auth.GoogleLogin;

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
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginWithPhoneOrEmailCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpPost("refresh-token")]
    [AllowAnonymous]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpPost("logout")]
    [Authorize("Citizen")]
    public async Task<IActionResult> Logout()
    {
        var result = await _mediator.Send(new LogoutCommand());
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpPost("register")]
    [Authorize("RequireRegistration")]
    public async Task<ActionResult> Register(RegisterUserCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpPost("verify-phone/request-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestPhoneVerificationOtp([FromBody] RequestPhoneVerificationOtpCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpPost("verify-phone/verify-otp")]
    [Authorize("RequirePhoneVerification")]
    public async Task<IActionResult> VerifyPhoneVerificationOtp([FromBody] PhoneVerificationCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpPost("verify-phone/resend-otp")]
    [Authorize("RequireOtpVerification")]
    public async Task<IActionResult> ResendPhoneVerificationOtp([FromBody] ResendPhoneVerificationOtpCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpPost("forget-password/request-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> RequestForgetPasswordOtp([FromBody] RequestForgetPasswordOtpCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpPost("forget-password/verify-otp")]
    [Authorize(Policy = "RequirePasswordResetVerification")]
    public async Task<IActionResult> VerifyForgetPasswordOtp([FromBody] VerifyForgetPasswordOtpCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpPost("forget-password/resend-otp")]
    [Authorize(Policy = "RequestResetPassword")]
    public async Task<IActionResult> ResendForgetPasswordOtp([FromBody] ResendForgetPasswordOtpCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpPost("forget-password/reset")]
    [Authorize(Policy = "RequireResetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }

    [HttpPost("google/login")]
    [AllowAnonymous]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginCommand command)
    {
        var result = await _mediator.Send(command);
        return result.ErrorList.Count != 0 ? BadRequest(result) : Ok(result);
    }
    
}