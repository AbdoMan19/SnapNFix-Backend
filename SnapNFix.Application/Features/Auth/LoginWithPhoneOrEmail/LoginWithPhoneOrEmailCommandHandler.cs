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

public class LoginWithPhoneOrEmailCommandHandler : IRequestHandler<LoginWithPhoneOrEmailCommand, GenericResponseModel<LoginResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginWithPhoneOrEmailCommandHandler> _logger;
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
        _userValidationService = userValidationService;
    }

    public async Task<GenericResponseModel<LoginResponse>> Handle(LoginWithPhoneOrEmailCommand request, CancellationToken cancellationToken)
    {
        var invalidCredentialsError = new List<ErrorResponseModel>
        {
            ErrorResponseModel.Create("Authentication", "Invalid credentials")
        };

        // Validate user
        var (user, error) = await _userValidationService.ValidateUserAsync<LoginResponse>(request.EmailOrPhoneNumber);
        if (error != null) return error;

        // Check password
        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            _logger.LogWarning("Invalid password attempt for user {UserId}", user.Id);
            await _userManager.AccessFailedAsync(user);
            return GenericResponseModel<LoginResponse>.Failure(Constants.FailureMessage, invalidCredentialsError);
        }

        // Check confirmation status
        var isEmail = request.EmailOrPhoneNumber.Contains("@");
        if ((isEmail && !user.EmailConfirmed) || (!isEmail && !user.PhoneNumberConfirmed))
        {
            _logger.LogWarning("Login attempt with unconfirmed {Type} for user {UserId}", 
                isEmail ? "email" : "phone", user.Id);
            
            return GenericResponseModel<LoginResponse>.Failure(
                Constants.FailureMessage,
                new List<ErrorResponseModel>{ 
                    ErrorResponseModel.Create("Authentication", $"{(isEmail ? "Email" : "Phone number")} not confirmed") 
                });
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        // Handle device information
        var userDevice = await _unitOfWork.Repository<UserDevice>()
            .FindBy(d => d.UserId == user.Id && d.DeviceId == request.DeviceId)
            .Include(u => u.RefreshToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (userDevice == null)
        {
            userDevice = new UserDevice
            {
                UserId = user.Id,
                DeviceId = request.DeviceId,
                DeviceName = request.DeviceName,
                DeviceType = request.DeviceType,
                Platform = request.Platform,
                LastUsedAt = DateTime.UtcNow,
            };
            await _unitOfWork.Repository<UserDevice>().Add(userDevice);
        }
        else
        {
            userDevice.LastUsedAt = DateTime.UtcNow;
            await _unitOfWork.Repository<UserDevice>().Update(userDevice);
        }

        // Check for active refresh token
        if (userDevice.RefreshToken != null && userDevice.RefreshToken.IsActive)
        {
            _logger.LogWarning("User device already exists with active refresh token for user {UserId}", user.Id);
            return GenericResponseModel<LoginResponse>.Failure("Device already logged in");
        }

        // Generate new tokens
        var accessToken = await _tokenService.GenerateJwtToken(user, userDevice);
        var refreshToken = _tokenService.GenerateRefreshToken();

        if (userDevice.RefreshToken != null)
        {
            // Update existing refresh token
            userDevice.RefreshToken.Token = refreshToken;
            userDevice.RefreshToken.Expires = _tokenService.GetRefreshTokenExpirationDays();
            await _unitOfWork.Repository<Domain.Entities.RefreshToken>().Update(userDevice.RefreshToken);
        }
        else
        {
            var refreshTokenObj = _tokenService.GenerateRefreshToken(userDevice);
            // Add new refresh token
            userDevice.RefreshToken = refreshTokenObj;
            await _unitOfWork.Repository<Domain.Entities.RefreshToken>().Add(refreshTokenObj);
        }

        // Single database call
        await _unitOfWork.SaveChanges();
        _logger.LogInformation("User {UserId} logged in successfully from device {DeviceId}", 
            user.Id, request.DeviceId);

        return GenericResponseModel<LoginResponse>.Success(new LoginResponse
        {
            Tokens = new AuthResponse
            {
                Token = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = _tokenService.GetTokenExpiration()
            },
            User = new LoginResponse.UserInfo
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed
            }
        });
    }
}