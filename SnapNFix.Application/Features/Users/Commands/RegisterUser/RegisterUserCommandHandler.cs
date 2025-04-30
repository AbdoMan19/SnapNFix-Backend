using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, GenericResponseModel<RegisterUserResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<RegisterUserCommandHandler> _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOtpService _otpService;
    private readonly ISmsService _smsService;
    private readonly ITokenService _tokenService;

    public RegisterUserCommandHandler(
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ILogger<RegisterUserCommandHandler> logger,
        IUnitOfWork unitOfWork,
        IOtpService otpService, 
        ISmsService smsService,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _otpService = otpService;
        _smsService = smsService;
        _tokenService = tokenService;
    }

    public async Task<GenericResponseModel<RegisterUserResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting registration process for phone number {PhoneNumber}", request.PhoneNumber);
        
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            UserName = request.PhoneNumber,
        };
        
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => ErrorResponseModel.Create(e.Code, e.Description)).ToList();
            _logger.LogWarning("User registration failed with {ErrorCount} errors", errors.Count);
            return GenericResponseModel<RegisterUserResponse>.Failure("Registration failed", errors);
        }

        var citizenRoleName = "Citizen";
        if (!await _roleManager.RoleExistsAsync(citizenRoleName))
        {
            _logger.LogInformation("Creating Citizen role as it doesn't exist");
            await _roleManager.CreateAsync(new IdentityRole<Guid>(citizenRoleName));
        }

        await _userManager.AddToRoleAsync(user, citizenRoleName);
        
        var otp = await _otpService.GenerateOtpAsync(request.PhoneNumber, OtpPurpose.PhoneVerification);
        _logger.LogInformation("Generated OTP for phone number {PhoneNumber}", request.PhoneNumber);
        
        var phoneNumberVerificationToken = await _tokenService.GeneratePhoneVerificationTokenAsync(user);
        _logger.LogInformation("Generated verification token for user {UserId}", user.Id);

        // Uncomment when ready to actually send SMS
        
        // var isSmsSent = await _smsService.SendSmsAsync(user.PhoneNumber, otp);
        // if (!isSmsSent)
        // {
        //     _logger.LogWarning("Failed to send OTP to phone number {PhoneNumber}", request.PhoneNumber);
        //     return GenericResponseModel<RegisterUserResponse>.Failure("Failed to send OTP. Please try again later.");
        // }

        _logger.LogInformation("OTP sent to phone number {PhoneNumber}", request.PhoneNumber);

        return GenericResponseModel<RegisterUserResponse>.Success(new RegisterUserResponse
        {
            UserId = user.Id,
            VerificationToken = phoneNumberVerificationToken
        });
    }
}