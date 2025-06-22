using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Common.Services.UserValidationServices;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Resources;
using SnapNFix.Application.Utilities;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Admin.Commands.AdminLogin;

public class AdminLoginCommandHandler : IRequestHandler<AdminLoginCommand, GenericResponseModel<LoginResponse>>
{
    private readonly ILogger<AdminLoginCommandHandler> _logger;
    private readonly IAuthenticationService _authenticationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly IUserValidationService _userValidationService;

    public AdminLoginCommandHandler(
        IUserValidationService userValidationService,
        UserManager<User> userManager,
        ILogger<AdminLoginCommandHandler> logger,
        IAuthenticationService authenticationService,
        IUnitOfWork unitOfWork)
    {
        _userValidationService = userValidationService;
        _userManager = userManager;
        _logger = logger;
        _authenticationService = authenticationService;
        _unitOfWork = unitOfWork;
    }

    public async Task<GenericResponseModel<LoginResponse>> Handle(
        AdminLoginCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Admin login attempt for email {Email}", request.Email);

        try
        {
            var invalidCredentialsError = new List<ErrorResponseModel>
            {
                ErrorResponseModel.Create("Authentication", Shared.InvalidCredentials)
            };

            var (user, error) = await _userValidationService.ValidateUserAsync<LoginResponse>(request.Email);
            if (error != null)
            {
                _logger.LogWarning("Admin login failed: {ErrorMessage}", error.ErrorList.FirstOrDefault()?.Message);
                return error;
            }

            var identityUser = await _userManager.FindByIdAsync(user.Id.ToString());
            if (identityUser == null)
            {
                _logger.LogWarning("Admin login failed: User not found in Identity system for email {Email}", request.Email);
                return GenericResponseModel<LoginResponse>.Failure(Constants.FailureMessage, invalidCredentialsError);
            }

            var passwordValid = await _userManager.CheckPasswordAsync(identityUser, request.Password);
            if (!passwordValid)
            {
                _logger.LogWarning("Admin login failed: Invalid password for email {Email}", request.Email);
                await _userManager.AccessFailedAsync(identityUser); 
                return GenericResponseModel<LoginResponse>.Failure(Constants.FailureMessage, invalidCredentialsError);
            }

            var userRoles = await _userManager.GetRolesAsync(identityUser);
            var hasAdminRole = userRoles.Contains("Admin") || userRoles.Contains("SuperAdmin");
            
            if (!hasAdminRole)
            {
                _logger.LogWarning("Admin login failed: User {UserId} does not have admin or super admin role", identityUser.Id);
                return GenericResponseModel<LoginResponse>.Failure(
                    Constants.FailureMessage,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create("Authorization", Shared.AdminRoleRequired)
                    });
            }

            if (!identityUser.EmailConfirmed)
            {
                _logger.LogWarning("Admin login failed: Email not confirmed for user {UserId}", identityUser.Id);
                return GenericResponseModel<LoginResponse>.Failure(
                    Constants.FailureMessage,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create("Authentication", Shared.EmailNotConfirmed)
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
                    string.Empty);

                await _unitOfWork.SaveChanges();
                await transaction.CommitAsync(cancellationToken);
                
                _logger.LogInformation("Admin {UserId} logged in successfully from device {DeviceId} with roles: {Roles}", 
                    identityUser.Id, request.DeviceId, string.Join(", ", userRoles));

                var loginResponse = new LoginResponse
                {
                    Tokens = authResponse,
                    User = identityUser.Adapt<LoginResponse.UserInfo>()
                };

                return GenericResponseModel<LoginResponse>.Success(loginResponse);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Database operation failed during admin login for user {UserId}", identityUser.Id);
                return GenericResponseModel<LoginResponse>.Failure(Shared.UnexpectedError);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin login failed for email {Email}", request.Email);
            return GenericResponseModel<LoginResponse>.Failure(Shared.UnexpectedError);
        }
    }
}

