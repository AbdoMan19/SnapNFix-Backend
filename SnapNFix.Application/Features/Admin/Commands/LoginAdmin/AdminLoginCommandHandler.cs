using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Utilities;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Admin.Commands.AdminLogin;

public class AdminLoginCommandHandler : IRequestHandler<AdminLoginCommand, GenericResponseModel<LoginResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AdminLoginCommandHandler> _logger;
    private readonly IAuthenticationService _authenticationService;
    private readonly IUnitOfWork _unitOfWork;

    public AdminLoginCommandHandler(
        UserManager<User> userManager,
        ILogger<AdminLoginCommandHandler> logger,
        IAuthenticationService authenticationService,
        IUnitOfWork unitOfWork)
    {
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
                ErrorResponseModel.Create("Authentication", "Invalid credentials or insufficient permissions")
            };

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || user.IsDeleted || user.IsSuspended)
            {
                _logger.LogWarning("Admin login failed: User not found or inactive for email {Email}", request.Email);
                return GenericResponseModel<LoginResponse>.Failure(Constants.FailureMessage, invalidCredentialsError);
            }

            var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!passwordValid)
            {
                _logger.LogWarning("Admin login failed: Invalid password for email {Email}", request.Email);
                await _userManager.AccessFailedAsync(user);
                return GenericResponseModel<LoginResponse>.Failure(Constants.FailureMessage, invalidCredentialsError);
            }

            if (!user.IsAdminUser)
            {
                _logger.LogWarning("Admin login failed: User {UserId} is not an admin user", user.Id);
                return GenericResponseModel<LoginResponse>.Failure(
                    Constants.FailureMessage,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create("Authorization", "Access denied: Admin privileges required")
                    });
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var hasAdminRole = userRoles.Contains("Admin") || userRoles.Contains("SuperAdmin");
            
            if (!hasAdminRole)
            {
                _logger.LogWarning("Admin login failed: User {UserId} does not have admin or super admin role", user.Id);
                return GenericResponseModel<LoginResponse>.Failure(
                    Constants.FailureMessage,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create("Authorization", "Access denied: Admin role required")
                    });
            }

            if (!user.EmailConfirmed)
            {
                _logger.LogWarning("Admin login failed: Email not confirmed for user {UserId}", user.Id);
                return GenericResponseModel<LoginResponse>.Failure(
                    Constants.FailureMessage,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create("Authentication", "Email address not confirmed")
                    });
            }

            await _userManager.ResetAccessFailedCountAsync(user);

            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            try
            {
                var authResponse = await _authenticationService.AuthenticateUserAsync(
                    user,
                    request.DeviceId,
                    request.DeviceName,
                    request.Platform,
                    request.DeviceType,
                    string.Empty);

                await _unitOfWork.SaveChanges();
                await transaction.CommitAsync(cancellationToken);
                
                _logger.LogInformation("Admin {UserId} logged in successfully from device {DeviceId} with roles: {Roles}", 
                    user.Id, request.DeviceId, string.Join(", ", userRoles));

                var loginResponse = new LoginResponse
                {
                    Tokens = authResponse,
                    User = user.Adapt<LoginResponse.UserInfo>()
                };

                return GenericResponseModel<LoginResponse>.Success(loginResponse);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Database operation failed during admin login for user {UserId}", user.Id);
                return GenericResponseModel<LoginResponse>.Failure("An error occurred during login");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Admin login failed for email {Email}", request.Email);
            return GenericResponseModel<LoginResponse>.Failure("An error occurred during login");
        }
    }
}