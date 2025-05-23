using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Common.Services.UserValidationServices;
using SnapNFix.Application.Features.Auth.Dtos;
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
    //device manager
    private readonly IDeviceManager _deviceManager;

    public LoginWithPhoneOrEmailCommandHandler(
        IUnitOfWork unitOfWork, 
        UserManager<User> userManager, 
        ITokenService tokenService,
        ILogger<LoginWithPhoneOrEmailCommandHandler> logger,
        IUserService userService,
        IUserValidationService userValidationService,
        IDeviceManager deviceManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
        _userValidationService = userValidationService;
        _deviceManager = deviceManager;
    }

    public async Task<GenericResponseModel<LoginResponse>> Handle(LoginWithPhoneOrEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var invalidCredentialsError = new List<ErrorResponseModel>
            {
                ErrorResponseModel.Create("Authentication", "Invalid credentials")
            };

            var (user, error) = await _userValidationService.ValidateUserAsync<LoginResponse>(request.EmailOrPhoneNumber);
            if (error != null) return error;

            var identityUser = await _userManager.FindByIdAsync(user.Id.ToString());
            
            var passwordValid = await _userManager.CheckPasswordAsync(identityUser, request.Password );
            if (!passwordValid)
            {
                _logger.LogWarning("Invalid password attempt for user {UserId}", identityUser.Id);
                await _userManager.AccessFailedAsync(identityUser);
                return GenericResponseModel<LoginResponse>.Failure(Constants.FailureMessage, invalidCredentialsError);
            }

            var isEmail = request.EmailOrPhoneNumber.Contains("@");
            if ((isEmail && !identityUser.EmailConfirmed) || (!isEmail && !identityUser.PhoneNumberConfirmed))
            {
                _logger.LogWarning("Login attempt with unconfirmed {Type} for user {UserId}", 
                    isEmail ? "email" : "phone", identityUser.Id);
                
                return GenericResponseModel<LoginResponse>.Failure(
                    Constants.FailureMessage,
                    new List<ErrorResponseModel>{ 
                        ErrorResponseModel.Create("Authentication", $"{(isEmail ? "Email" : "Phone number")} not confirmed") 
                    });
            }

            await _userManager.ResetAccessFailedCountAsync(identityUser);

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
                        identityUser.Id, request.DeviceId);
                }
                else
                {
                    userDevice.LastUsedAt = DateTime.UtcNow;
                    await _unitOfWork.Repository<UserDevice>().Update(userDevice);
                }

                var accessToken = await _tokenService.GenerateJwtToken(identityUser, userDevice);
                string refreshTokenString;

                if (userDevice.RefreshToken != null)
                {
                    Console.WriteLine("Refresh token exists, updating it");
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
                    identityUser.Id, request.DeviceId);

                return GenericResponseModel<LoginResponse>.Success(new LoginResponse
                {
                    Tokens = new AuthResponse
                    {
                        Token = accessToken,
                        RefreshToken = refreshTokenString,
                        ExpiresAt = userDevice.RefreshToken.Expires
                    },
                    User = new LoginResponse.UserInfo
                    {
                        Id = identityUser.Id,
                        FirstName = identityUser.FirstName,
                        LastName = identityUser.LastName,
                        Email = identityUser.Email,
                        PhoneNumber = identityUser.PhoneNumber,
                        PhoneNumberConfirmed = identityUser.PhoneNumberConfirmed
                    }
                });
            }
            catch (Exception ex)
            {
                // Rollback on error
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Database operation failed during login for user with ID {UserId}", identityUser?.Id);
                return GenericResponseModel<LoginResponse>.Failure("An error occurred during login");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for {EmailOrPhone}", request.EmailOrPhoneNumber);
            return GenericResponseModel<LoginResponse>.Failure("An error occurred during login");
        }
    }
}