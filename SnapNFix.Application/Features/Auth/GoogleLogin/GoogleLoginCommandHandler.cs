using Google.Apis.Auth;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Interfaces;
using SnapNFix.Application.Resources;
using SnapNFix.Application.Utilities;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Auth.GoogleLogin;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, GenericResponseModel<LoginResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GoogleLoginCommandHandler> _logger;
    private readonly IAuthenticationService _authenticationService;
    private readonly ICacheInvalidationService _cacheInvalidationService;

    public GoogleLoginCommandHandler(
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        IConfiguration configuration,
        IUnitOfWork unitOfWork,
        ILogger<GoogleLoginCommandHandler> logger,
        IAuthenticationService authenticationService,
        ICacheInvalidationService cacheInvalidationService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _unitOfWork = unitOfWork;
        _logger = logger;
        _authenticationService = authenticationService;
        _cacheInvalidationService = cacheInvalidationService;
    }

    public async Task<GenericResponseModel<LoginResponse>> Handle(
        GoogleLoginCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing Google login for device {DeviceId}", request.DeviceId);

        try
        {
            var payload = await ValidateGoogleTokenAsync(request.IdToken);
            if (payload == null)
            {
                _logger.LogWarning("Google token validation failed for device {DeviceId}", request.DeviceId);
                return GenericResponseModel<LoginResponse>.Failure(
                    Constants.FailureMessage,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create("Authentication", Shared.InvalidGoogleToken)
                    });
            }

            _logger.LogInformation("Google token validated successfully for email {Email}", payload.Email);

            var user = await FindOrCreateUserAsync(payload);
            if (user == null)
            {
                _logger.LogError("Failed to create or find user for Google login with email {Email}", payload.Email);
                return GenericResponseModel<LoginResponse>.Failure(Shared.RegistrationFailed,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create("Authentication", Shared.EmailAlreadyRegistered)
                    });
            }

            if (user.IsDeleted)
            {
                _logger.LogWarning("Deleted user attempted Google login: {UserId}", user.Id);
                return GenericResponseModel<LoginResponse>.Failure(
                    Constants.FailureMessage,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create("Authentication", Shared.UserNotFound)
                    });
            }

            if (user.IsSuspended)
            {
                _logger.LogWarning("Suspended user attempted Google login: {UserId}", user.Id);
                return GenericResponseModel<LoginResponse>.Failure(
                    Constants.FailureMessage,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create("Authentication", Shared.AccountSuspended)
                    });
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            try
            {
                var authResponse = await _authenticationService.AuthenticateUserAsync(
                    user,
                    request.DeviceId,
                    request.DeviceName,
                    request.Platform,
                    request.DeviceType,
                    request.FCMToken);

                await _unitOfWork.SaveChanges();
                await transaction.CommitAsync(cancellationToken);

                // Step 5: Invalidate user cache
                await _cacheInvalidationService.InvalidateUserCacheAsync(user.Id);

                _logger.LogInformation("Google login successful for user {UserId} on device {DeviceId}", 
                    user.Id, request.DeviceId);

                return GenericResponseModel<LoginResponse>.Success(new LoginResponse
                {
                    Tokens = authResponse,
                    User = user.Adapt<LoginResponse.UserInfo>()
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Database operation failed during Google login for user {UserId}", user.Id);
                return GenericResponseModel<LoginResponse>.Failure(Shared.UnexpectedError,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create("Database", Shared.UnexpectedError)
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Google login failed for device {DeviceId}", request.DeviceId);
            return GenericResponseModel<LoginResponse>.Failure(Shared.UnexpectedError,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create("Authentication", Shared.UnexpectedError)
                });
        }
    }

    private async Task<GoogleJsonWebSignature.Payload?> ValidateGoogleTokenAsync(string idToken)
    {
        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["Authentication:Google:ClientId"] }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            
            // Additional validation
            if (payload == null || string.IsNullOrEmpty(payload.Email))
            {
                _logger.LogWarning("Google token validation returned null or empty email");
                return null;
            }

            if (!payload.EmailVerified)
            {
                _logger.LogWarning("Google account email not verified for {Email}", payload.Email);
                return null;
            }

            return payload;
        }
        catch (InvalidJwtException ex)
        {
            _logger.LogWarning(ex, "Invalid Google JWT token");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Google token");
            return null;
        }
    }

    private async Task<User?> FindOrCreateUserAsync(GoogleJsonWebSignature.Payload payload)
    {
        var existingUser = await _userManager.FindByEmailAsync(payload.Email);
        if (existingUser != null)
        {
            var hasChanges = false;

            if (string.IsNullOrEmpty(existingUser.FirstName) && !string.IsNullOrEmpty(payload.GivenName))
            {
                existingUser.FirstName = payload.GivenName;
                hasChanges = true;
            }

            if (string.IsNullOrEmpty(existingUser.LastName) && !string.IsNullOrEmpty(payload.FamilyName))
            {
                existingUser.LastName = payload.FamilyName;
                hasChanges = true;
            }

            if (!existingUser.EmailConfirmed)
            {
                existingUser.EmailConfirmed = payload.EmailVerified;
                hasChanges = true;
            }

            if (hasChanges)
            {
                existingUser.UpdatedAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(existingUser);
                _logger.LogInformation("Updated existing user {UserId} with Google profile info", existingUser.Id);
            }

            return existingUser;
        }

        var newUser = new User
        {
            FirstName = payload.GivenName ?? "User",
            LastName = payload.FamilyName ?? "Google",
            Email = payload.Email,
            UserName = payload.Email,
            EmailConfirmed = payload.EmailVerified,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var randomPassword = PasswordGenerator.GenerateStrongPassword();
        var result = await _userManager.CreateAsync(newUser, randomPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogError("Failed to create user from Google login: {Errors}", errors);
            return null;
        }

        await EnsureRoleExistsAsync("Citizen");
        await _userManager.AddToRoleAsync(newUser, "Citizen");

        _logger.LogInformation("Created new user {UserId} from Google login with email {Email}", 
            newUser.Id, newUser.Email);

        return newUser;
    }

    private async Task EnsureRoleExistsAsync(string roleName)
    {
        if (!await _roleManager.RoleExistsAsync(roleName))
        {
            _logger.LogInformation("Creating role {RoleName} as it doesn't exist", roleName);
            await _roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
        }
    }
}

