using Google.Apis.Auth;
using Mapster;
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
        try
        {
            // Step 1: Validate Google token (no DB operation)
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["Authentication:Google:ClientId"] }
            };
            
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.AccessToken, settings);
            if (payload == null)
            {
                _logger.LogWarning("Google authentication failed - invalid token");
                return GenericResponseModel<LoginResponse>.Failure("Invalid Google token");
            }

            _logger.LogInformation("Google authentication successful for email {Email}", payload.Email);

            // Step 2: Find or create user (UserManager has its own transaction management)
            var user = await _userManager.FindByEmailAsync(payload.Email);
            if (user == null)
            {
                user = new User
                {
                    Email = payload.Email,
                    FirstName = payload.GivenName,
                    LastName = payload.FamilyName,
                    EmailConfirmed = payload.EmailVerified,
                    UserName = payload.Email
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

            // Step 3: Start an explicit transaction for our operations
            // Start a transaction for all database operations
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            try
            {
                var userDevice = await _deviceManager.RegisterDeviceAsync(
                    user.Id,
                    request.DeviceId,
                    request.DeviceName,
                    request.Platform,
                    request.DeviceType);

                bool isNewDevice = userDevice.Id == Guid.Empty;
                
                if (isNewDevice)
                {
                    await _unitOfWork.Repository<UserDevice>().Add(userDevice);
                    // Save now only if we need the ID for child entities
                    await _unitOfWork.SaveChanges();
                    
                    _logger.LogInformation("New device registered for user {UserId} with device ID {DeviceId}",
                        user.Id, request.DeviceId);
                }
                else
                {
                    userDevice.LastUsedAt = DateTime.UtcNow;
                    await _unitOfWork.Repository<UserDevice>().Update(userDevice);
                }

                var accessToken = await _tokenService.GenerateJwtToken(user, userDevice);
                string refreshTokenString;

                if (userDevice.RefreshToken != null)
                {
                    refreshTokenString = _tokenService.GenerateRefreshToken();
                    userDevice.RefreshToken.Token = refreshTokenString;
                    userDevice.RefreshToken.Expires = _tokenService.GetRefreshTokenExpirationDays();
                    userDevice.RefreshToken.CreatedAt = DateTime.UtcNow;
                    await _unitOfWork.Repository<Domain.Entities.RefreshToken>().Update(userDevice.RefreshToken);
                }
                else
                {
                    var refreshTokenObj = new Domain.Entities.RefreshToken
                    {
                        Token = _tokenService.GenerateRefreshToken(),
                        UserDeviceId = userDevice.Id,
                        Expires = _tokenService.GetRefreshTokenExpirationDays(),
                        CreatedAt = DateTime.UtcNow
                    };
                    refreshTokenString = refreshTokenObj.Token;
                    await _unitOfWork.Repository<Domain.Entities.RefreshToken>().Add(refreshTokenObj);
                    userDevice.RefreshToken = refreshTokenObj;
                }

                // Single SaveChanges() for all pending changes
                await _unitOfWork.SaveChanges();
                
                // Commit the transaction
                await transaction.CommitAsync(cancellationToken);
                
                _logger.LogInformation("User {UserId} logged in successfully from device {DeviceId}", 
                    user.Id, request.DeviceId);

                return GenericResponseModel<LoginResponse>.Success(new LoginResponse
                {
                    Tokens = new AuthResponse
                    {
                        Token = accessToken,
                        RefreshToken = refreshTokenString,
                        ExpiresAt = userDevice.RefreshToken.Expires
                    },
                    User = user.Adapt<LoginResponse.UserInfo>()
                });
            }
            catch (Exception ex)
            {
                // Rollback on error
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Database operation failed during login for user with ID {UserId}", user.Id);
                return GenericResponseModel<LoginResponse>.Failure("An error occurred during login");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google login failed for token beginning with {TokenPrefix}", 
                request.AccessToken.Length > 10 ? request.AccessToken.Substring(0, 10) + "..." : "[empty]");
            return GenericResponseModel<LoginResponse>.Failure("An error occurred during login");
        }
    }
}