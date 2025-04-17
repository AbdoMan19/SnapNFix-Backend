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
    private readonly ISmsService _smsService;

    public RegisterUserCommandHandler(
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> _roleManager,
        ILogger<RegisterUserCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IOtpService otpService,
        ISmsService smsService)
    {
        _userManager = userManager;
        this._roleManager = _roleManager;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _otpService = otpService;
        _smsService = smsService;
    }

    public async Task<GenericResponseModel<Guid>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting registration process for phone number {PhoneNumber}", request.PhoneNumber);
        
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            UserName = Guid.NewGuid().ToString(),
            PhoneNumberConfirmed = false,
            Email = string.Empty,
            NormalizedEmail = string.Empty,
            EmailConfirmed = false,       
            IsDeleted = false
        };
        
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

        // send otp to user phone number
        var otp = await _otpService.GenerateOtpAsync(user.PhoneNumber);
        _logger.LogInformation("Generated OTP for phone number {PhoneNumber}", request.PhoneNumber);
        
        
        var isSmsSent = await _smsService.SendSmsAsync(user.PhoneNumber, otp);

        if (!isSmsSent)
        {
            _logger.LogWarning("Failed to send OTP to phone number {PhoneNumber}", request.PhoneNumber);
            return GenericResponseModel<Guid>.Failure("Failed to send OTP. Please try again later.");
        }

        _logger.LogInformation("OTP sent to phone number {PhoneNumber}", request.PhoneNumber);

        return GenericResponseModel<Guid>.Success(user.Id);
    }
}