using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Common.Services;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;
using Constants = SnapNFix.Application.Utilities.Constants;

namespace SnapNFix.Application.Features.Auth.LoginWithPhoneOrEmail;

public class LoginWithPhoneOrEmailCommandHandler : IRequestHandler<LoginWithPhoneOrEmailCommand, GenericResponseModel<AuthResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginWithPhoneOrEmailCommandHandler> _logger;
    private readonly IUserService _userService;
    private readonly IUserValidationService _userValidationService;


    public LoginWithPhoneOrEmailCommandHandler(
        IUnitOfWork unitOfWork, 
        UserManager<User> userManager, 
        ITokenService tokenService,
        ILogger<LoginWithPhoneOrEmailCommandHandler> logger,
        IUserService userService,
        IUserValidationService userValidationService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
        _userService = userService;
        _userValidationService = userValidationService;
        
    }

    public async Task<GenericResponseModel<AuthResponse>> Handle(LoginWithPhoneOrEmailCommand request, CancellationToken cancellationToken)
    {
        var invalidCredentialsError = new List<ErrorResponseModel>
        {
            ErrorResponseModel.Create("Authentication", "Invalid credentials")
        };
        var(user,error) = await _userValidationService.ValidateUserAsync<AuthResponse>(request.EmailOrPhoneNumber);
        if(error != null)
        {
            return error;
        }
        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            _logger.LogWarning("Invalid password attempt for user {UserId}", user.Id);
            
            await _userManager.AccessFailedAsync(user);
            
            return GenericResponseModel<AuthResponse>.Failure(Constants.FailureMessage, invalidCredentialsError);
        }
        var isEmail = request.EmailOrPhoneNumber.Contains("@");
        if (isEmail && !user.EmailConfirmed ||
            !isEmail && !user.PhoneNumberConfirmed)
        {
            _logger.LogWarning("Login attempt with unconfirmed email for user {UserId}", user.Id);
            return GenericResponseModel<AuthResponse>.Failure(
                Constants.FailureMessage,
                new List<ErrorResponseModel>{ ErrorResponseModel.Create("Authentication", "EmailOrPhoneNumber not confirmed") });
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var token = await _tokenService.GenerateJwtToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken(user);

        _unitOfWork.Repository<Domain.Entities.RefreshToken>().Add(refreshToken);
        await _unitOfWork.SaveChanges();

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return GenericResponseModel<AuthResponse>.Success(new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken.Token,
            ExpiresAt = _tokenService.GetTokenExpiration(),
        });
    }
}