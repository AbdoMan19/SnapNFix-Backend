using Constants = SnapNFix.Application.Utilities.Constants;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Common.Services.UserValidationServices;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Interfaces;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Auth.LoginWithPhoneOrEmail;

public class LoginWithPhoneOrEmailCommandHandler : IRequestHandler<LoginWithPhoneOrEmailCommand, GenericResponseModel<LoginResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<LoginWithPhoneOrEmailCommandHandler> _logger;
    private readonly IUserValidationService _userValidationService;
    private readonly IAuthenticationService _authenticationService;
    private readonly IDeviceManager _deviceManager;

    public LoginWithPhoneOrEmailCommandHandler(
        IUnitOfWork unitOfWork, 
        UserManager<User> userManager, 
        ITokenService tokenService,
        ILogger<LoginWithPhoneOrEmailCommandHandler> logger,
        IUserValidationService userValidationService,
        IAuthenticationService authenticationService,
        IDeviceManager deviceManager)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
        _userValidationService = userValidationService;
        _authenticationService = authenticationService;
        _deviceManager = deviceManager;
    }

    public async Task<GenericResponseModel<LoginResponse>> Handle(LoginWithPhoneOrEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var invalidCredentialsError = new List<ErrorResponseModel>
            {
                ErrorResponseModel.Create("Authentication", Shared.InvalidCredentials)
            };

            var (user, error) = await _userValidationService.ValidateUserAsync<LoginResponse>(request.EmailOrPhoneNumber);
            if (error != null) return error;

            var identityUser = await _userManager.FindByIdAsync(user.Id.ToString());
            
            var passwordValid = await _userManager.CheckPasswordAsync(identityUser, request.Password);
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
                
                var confirmationMessage = isEmail ? Shared.EmailNotConfirmed : Shared.PhoneNotConfirmed;
                return GenericResponseModel<LoginResponse>.Failure(
                    Constants.FailureMessage,
                    new List<ErrorResponseModel>{ 
                        ErrorResponseModel.Create("Authentication", confirmationMessage) 
                    });
            }

            await _userManager.ResetAccessFailedCountAsync(identityUser);

            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            try
            {
                var authResponse = await _authenticationService.AuthenticateUserAsync(
                    identityUser,
                    request.DeviceId,
                    request.DeviceName,
                    request.Platform,
                    request.DeviceType,
                    request.FCMToken);

                await _unitOfWork.SaveChanges();
                await transaction.CommitAsync(cancellationToken);
                
                _logger.LogInformation("User {UserId} logged in successfully from device {DeviceId}", 
                    identityUser.Id, request.DeviceId);

                return GenericResponseModel<LoginResponse>.Success(new LoginResponse
                {
                    Tokens = authResponse,
                    User = identityUser.Adapt<LoginResponse.UserInfo>()
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Database operation failed during login for user with ID {UserId}", identityUser?.Id);
                return GenericResponseModel<LoginResponse>.Failure(Shared.UnexpectedError,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create("Database", Shared.UnexpectedError)
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login failed for {EmailOrPhone}", request.EmailOrPhoneNumber);
            return GenericResponseModel<LoginResponse>.Failure(Shared.UnexpectedError,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create(nameof(request.EmailOrPhoneNumber), Shared.UnexpectedError)
                });
        }
    }
}

