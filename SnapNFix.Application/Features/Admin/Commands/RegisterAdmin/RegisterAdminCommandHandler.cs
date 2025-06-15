using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Admin.Commands.RegisterAdmin;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Application.Utilities;

namespace SnapNFix.Application.Features.Admin.Commands.RegisterAdmin;

public class RegisterAdminCommandHandler : IRequestHandler<RegisterAdminCommand, GenericResponseModel<AdminRegistrationResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<RegisterAdminCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuthenticationService _authenticationService;

    public RegisterAdminCommandHandler(
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ILogger<RegisterAdminCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IAuthenticationService authenticationService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _authenticationService = authenticationService;
    }

    public async Task<GenericResponseModel<AdminRegistrationResponse>> Handle(
        RegisterAdminCommand request, 
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting admin registration process for email {Email}", request.Email);

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                _logger.LogWarning("Admin registration failed: Email {Email} already exists", request.Email);
                return GenericResponseModel<AdminRegistrationResponse>.Failure(
                    Constants.FailureMessage,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create("Email", "Email address is already registered")
                    });
            }

            var adminUser = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email, 
                EmailConfirmed = true, 
                IsAdminUser = true,
                PhoneNumber = null 
            };

            var result = await _userManager.CreateAsync(adminUser, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => ErrorResponseModel.Create(e.Code, e.Description)).ToList();
                _logger.LogWarning("Admin user creation failed with {ErrorCount} errors", errors.Count);
                return GenericResponseModel<AdminRegistrationResponse>.Failure(
                    "Admin registration failed", errors);
            }

            const string adminRoleName = "Admin";
            if (!await _roleManager.RoleExistsAsync(adminRoleName))
            {
                _logger.LogInformation("Creating Admin role as it doesn't exist");
                await _roleManager.CreateAsync(new IdentityRole<Guid>(adminRoleName));
            }

            await _userManager.AddToRoleAsync(adminUser, adminRoleName);

            var authResponse = await _authenticationService.AuthenticateUserAsync(
                adminUser,
                request.DeviceId,
                request.DeviceName,
                request.Platform,
                request.DeviceType,
                "");

            await _unitOfWork.SaveChanges();
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Admin {AdminId} registered successfully with email {Email}",
                adminUser.Id, request.Email);

            var response = new AdminRegistrationResponse
            {
                AdminId = adminUser.Id,
                Email = adminUser.Email,
                FirstName = adminUser.FirstName,
                LastName = adminUser.LastName,
                CreatedAt = adminUser.CreatedAt,
                Tokens = authResponse
            };

            return GenericResponseModel<AdminRegistrationResponse>.Success(response);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Admin registration failed with error for email {Email}", request.Email);
            return GenericResponseModel<AdminRegistrationResponse>.Failure(
                "An error occurred during admin registration");
        }
    }
}