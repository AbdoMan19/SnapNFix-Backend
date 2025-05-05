using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Users.Commands.RegisterUser;

public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, GenericResponseModel<LoginResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole<Guid>> _roleManager;
    private readonly ILogger<RegisterUserCommandHandler> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenService _tokenService;

    public RegisterUserCommandHandler(
        UserManager<User> userManager,
        RoleManager<IdentityRole<Guid>> roleManager,
        ILogger<RegisterUserCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor,
        IUnitOfWork unitOfWork,
        ITokenService tokenService)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _unitOfWork = unitOfWork;
        _tokenService = tokenService;
    }

    public async Task<GenericResponseModel<LoginResponse>> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
    {
        var phoneNumber = _httpContextAccessor.HttpContext?.User.FindFirst("phone")?.Value;

        _logger.LogInformation("Starting registration process for phone number {PhoneNumber}", phoneNumber);
        
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = phoneNumber,
            UserName = phoneNumber,
            PhoneNumberConfirmed = true,
        };
        
        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => ErrorResponseModel.Create(e.Code, e.Description)).ToList();
            _logger.LogWarning("User registration failed with {ErrorCount} errors", errors.Count);
            return GenericResponseModel<LoginResponse>.Failure("Registration failed", errors);
        }

        var citizenRoleName = "Citizen";
        if (!await _roleManager.RoleExistsAsync(citizenRoleName))
        {
            _logger.LogInformation("Creating Citizen role as it doesn't exist");
            await _roleManager.CreateAsync(new IdentityRole<Guid>(citizenRoleName));
        }

        await _userManager.AddToRoleAsync(user, citizenRoleName);
        
        //add user device to the user and generate the tokens
        var userDevice = new UserDevice
        {
            DeviceId = request.DeviceId,
            DeviceName = request.DeviceName,
            DeviceType = request.DeviceType,
            Platform = request.Platform,
            UserId = user.Id //55
        };
        
        // Generate new tokens
        var accessToken = await _tokenService.GenerateJwtToken(user, userDevice);
        var refreshTokenObj = _tokenService.GenerateRefreshToken(userDevice);
        // Add new refresh token
        userDevice.RefreshToken = refreshTokenObj;
        await _unitOfWork.Repository<UserDevice>().Add(userDevice);
        await _unitOfWork.Repository<RefreshToken>().Add(refreshTokenObj);
        

        // Single database call
        await _unitOfWork.SaveChanges();
        _logger.LogInformation("User {UserId} logged in successfully from device {DeviceId}", 
            user.Id, request.DeviceId);

        return GenericResponseModel<LoginResponse>.Success(new LoginResponse
        {
            Tokens = new AuthResponse
            {
                Token = accessToken,
                RefreshToken = refreshTokenObj.Token,
                ExpiresAt = _tokenService.GetTokenExpiration()
            },
            User = new LoginResponse.UserInfo
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed
            }
        });
    }
}