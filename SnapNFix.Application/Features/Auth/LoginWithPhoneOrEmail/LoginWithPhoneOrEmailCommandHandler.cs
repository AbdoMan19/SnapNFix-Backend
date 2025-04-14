using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using Constants = SnapNFix.Application.Utilities.Constants;

namespace SnapNFix.Application.Features.Auth.LoginWithPhoneOrEmail;

public class LoginWithPhoneOrEmailCommandHandler : IRequestHandler<LoginWithPhoneOrEmailCommand, GenericResponseModel<AuthResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginWithPhoneOrEmailCommandHandler> _logger;


    public LoginWithPhoneOrEmailCommandHandler(
        IUnitOfWork unitOfWork, 
        UserManager<User> userManager, 
        ITokenService tokenService,
        ILogger<LoginWithPhoneOrEmailCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<AuthResponse>> Handle(LoginWithPhoneOrEmailCommand request, CancellationToken cancellationToken)
    {
        var invalidCredentialsError = new List<ErrorResponseModel>
        {
            ErrorResponseModel.Create("Authentication", "Invalid credentials")
        };

        string identifier = !string.IsNullOrEmpty(request.Email) 
            ? request.Email 
            : request.PhoneNumber;
            
        if (string.IsNullOrEmpty(identifier))
        {
            _logger.LogWarning("Login attempt failed: No identifier provided");
            return GenericResponseModel<AuthResponse>.Failure(
                Constants.FailureMessage, 
                new List<ErrorResponseModel>{ ErrorResponseModel.Create("Authentication", "Email or phone number is required") });
        }

        var user = await _unitOfWork.Repository<User>()
            .FindBy(u => 
                (!string.IsNullOrEmpty(request.Email) && u.Email == request.Email) || 
                (!string.IsNullOrEmpty(request.PhoneNumber) && u.PhoneNumber == request.PhoneNumber))
            .FirstOrDefaultAsync(cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("Login attempt failed: User not found for identifier {Identifier}", identifier);
            return GenericResponseModel<AuthResponse>.Failure(Constants.FailureMessage, invalidCredentialsError);
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            _logger.LogWarning("Login attempt for locked account: {UserId}", user.Id);
            return GenericResponseModel<AuthResponse>.Failure(
                Constants.FailureMessage, 
                new List<ErrorResponseModel>{ ErrorResponseModel.Create("Authentication", "Account is temporarily locked") });
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            _logger.LogWarning("Invalid password attempt for user {UserId}", user.Id);
            
            await _userManager.AccessFailedAsync(user);
            
            return GenericResponseModel<AuthResponse>.Failure(Constants.FailureMessage, invalidCredentialsError);
        }

        if (_userManager.Options.SignIn.RequireConfirmedEmail && !user.EmailConfirmed)
        {
            _logger.LogWarning("Login attempt with unconfirmed email for user {UserId}", user.Id);
            return GenericResponseModel<AuthResponse>.Failure(
                Constants.FailureMessage,
                new List<ErrorResponseModel>{ ErrorResponseModel.Create("Authentication", "Email address not confirmed") });
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var token = await _tokenService.GenerateJwtToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();
        
        await _tokenService.SaveRefreshTokenAsync(user, refreshToken, request.IpAddress);

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        return GenericResponseModel<AuthResponse>.Success(new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = _tokenService.GetTokenExpiration(),
        });
    }
}