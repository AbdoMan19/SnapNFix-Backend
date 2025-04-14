using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, GenericResponseModel<Guid>>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<RegisterUserCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpService _otpService;

    public RegisterUserCommandHandler(
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> _roleManager,
        ILogger<RegisterUserCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IOtpService otpService)
    {
        _userManager = userManager;
        this._roleManager = _roleManager;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _otpService = otpService;
    }

    public async Task<GenericResponseModel<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting registration process for phone number {PhoneNumber}", request.PhoneNumber);
        
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            // UserName = request.Email ?? request.PhoneNumber, 
            PhoneNumberConfirmed = false,
            EmailConfirmed = false,       
            // ImagePath = "defaults/user-profile.png", 
            IsDeleted = false
        };

        await _unitOfWork.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted);
        try
        {
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => ErrorResponseModel.Create(e.Code, e.Description)).ToList();
                _logger.LogWarning("User registration failed with {ErrorCount} errors", errors.Count);
                return GenericResponseModel<Guid>.Failure("Registration failed", errors);
            }

            var citizenRoleName = "Citizen";
            if (!await _roleManager.RoleExistsAsync(citizenRoleName))
            {
                _logger.LogInformation("Creating Citizen role as it doesn't exist");
                await _roleManager.CreateAsync(new IdentityRole<Guid>(citizenRoleName));
            }

            await _userManager.AddToRoleAsync(user, citizenRoleName);

            if (!string.IsNullOrEmpty(request.PhoneNumber))
            {
                await _otpService.GenerateOtpAsync(request.PhoneNumber);
                _logger.LogInformation("OTP sent to phone number {PhoneNumber}", request.PhoneNumber);
            }

            await _unitOfWork.CommitTransactionAsync();
            
            _logger.LogInformation("User {UserId} registered successfully", user.Id);
            return GenericResponseModel<Guid>.Success(user.Id);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollBackTransactionAsync();
            _logger.LogError(ex, "Error during user registration");
            return GenericResponseModel<Guid>.Failure("An unexpected error occurred during registration");
        }
    }
}