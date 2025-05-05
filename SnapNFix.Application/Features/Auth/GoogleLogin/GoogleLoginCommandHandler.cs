using Google.Apis.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Utilities;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Auth.GoogleLogin;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, GenericResponseModel<LoginResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GoogleLoginCommandHandler> _logger;
    private readonly IDeviceManager _deviceManager;

    public GoogleLoginCommandHandler(
        UserManager<User> userManager,
        ITokenService tokenService,
        IConfiguration configuration,
        IUnitOfWork unitOfWork,
        ILogger<GoogleLoginCommandHandler> logger,
        IDeviceManager deviceManager)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _deviceManager = deviceManager;
    }

    public async Task<GenericResponseModel<LoginResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _configuration["Authentication:Google:ClientId"] }
        };
        
        var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        if (payload == null)
        {
            _logger.LogWarning("Google authentication failed - invalid token");
            return GenericResponseModel<LoginResponse>.Failure("Invalid Google token");
        }

        _logger.LogInformation("Google authentication successful for email {Email}", payload.Email);

        var user = await _userManager.FindByEmailAsync(payload.Email);
        if (user == null)
        {
            user = new User
            {
                Email = payload.Email,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                EmailConfirmed = payload.EmailVerified,
                UserName = payload.Email // Ensure UserName is set
            };

            var randomPassword = PasswordGenerator.GenerateStrongPassword();
            var result = await _userManager.CreateAsync(user, randomPassword);
            
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => ErrorResponseModel.Create(e.Code, e.Description)).ToList();
                _logger.LogWarning("Failed to create user from Google login with {ErrorCount} errors", errors.Count);
                return GenericResponseModel<LoginResponse>.Failure(Constants.FailureMessage, errors);
            }

            await _userManager.AddToRoleAsync(user, "Citizen");
            _logger.LogInformation("Created new user from Google login: {UserId}", user.Id);
        }

        var userDevice = await _deviceManager.RegisterDeviceAsync(
            user.Id,
            request.DeviceId,
            request.DeviceName,
            request.Platform,
            request.DeviceType);

        if (userDevice.RefreshToken != null && userDevice.RefreshToken.IsActive)
        {
            _logger.LogWarning("User device already exists with active refresh token for user {UserId}", user.Id);
            return GenericResponseModel<LoginResponse>.Failure("Device already logged in");
        }

        var accessToken = await _tokenService.GenerateJwtToken(user, userDevice);
        var refreshToken = _tokenService.GenerateRefreshToken(userDevice);
        
        if (userDevice.RefreshToken != null)
        {
            userDevice.RefreshToken.Token = refreshToken.Token;
            userDevice.RefreshToken.Expires = _tokenService.GetRefreshTokenExpirationDays();
            await _unitOfWork.Repository<Domain.Entities.RefreshToken>().Update(userDevice.RefreshToken);
        }
        else
        {
            userDevice.RefreshToken = refreshToken;
            await _unitOfWork.Repository<Domain.Entities.RefreshToken>().Add(refreshToken);
        }

        userDevice.LastUsedAt = DateTime.UtcNow;
        await _unitOfWork.Repository<UserDevice>().Update(userDevice);

        await _unitOfWork.SaveChanges();

        _logger.LogInformation("Google login successful for user {UserId} from device {DeviceId}", 
            user.Id, request.DeviceId);

            return GenericResponseModel<LoginResponse>.Success(new LoginResponse
            {
                Tokens = new AuthResponse
                {
                    Token = accessToken,
                    RefreshToken = refreshToken.Token,
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