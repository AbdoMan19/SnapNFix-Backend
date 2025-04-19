using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Common.Services;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Interfaces;
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
    private readonly IUserService _userService;
    private readonly IUserValidationService _userValidationService;

    public LoginWithPhoneOrEmailCommandHandler(
        IUnitOfWork unitOfWork, 
        UserManager<User> userManager, 
        ITokenService tokenService,
        ILogger<LoginWithPhoneOrEmailCommandHandler> logger,
        IUserService userService,
        IUserValidationService userValidationService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
        _userService = userService;
        _userValidationService = userValidationService;
        
    }

    public async Task<GenericResponseModel<LoginResponse>> Handle(LoginWithPhoneOrEmailCommand request, CancellationToken cancellationToken)
    {
        var invalidCredentialsError = new List<ErrorResponseModel>
        {
            ErrorResponseModel.Create("Authentication", "Invalid credentials")
        };
        var (user, error) = await _userValidationService.ValidateUserAsync<LoginResponse>(request.EmailOrPhoneNumber);

        if (error != null)
        {
            return error;
        }

        

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!passwordValid)
        {
            _logger.LogWarning("Invalid password attempt for user {UserId}", user.Id);
            
            await _userManager.AccessFailedAsync(user);
            
            return GenericResponseModel<LoginResponse>.Failure(Constants.FailureMessage, invalidCredentialsError);
        }

        var isEmail = request.EmailOrPhoneNumber.Contains("@");
        if (isEmail && !user.EmailConfirmed ||
            !isEmail && !user.PhoneNumberConfirmed)
        {
            _logger.LogWarning("Login attempt with unconfirmed email for user {UserId}", user.Id);
            return GenericResponseModel<LoginResponse>.Failure(
                Constants.FailureMessage,
                new List<ErrorResponseModel>{ ErrorResponseModel.Create("Authentication", "EmailOrPhoneNumber not confirmed") });
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var newAccesstoken = await _tokenService.GenerateJwtToken(user);
        var newRefreshToken = _tokenService.GenerateRefreshToken(user);
        
        var oldRefreshToken = await _unitOfWork.Repository<Domain.Entities.RefreshToken>().FindBy(u => u.UserId == user.Id)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (oldRefreshToken != null)
        {
            // Store the ID before deleting
            var tokenIdToDelete = oldRefreshToken.Id;

            // Clear the reference first
            user.RefreshToken = null;
            user.RefreshTokenId = null;
            await _unitOfWork.Repository<User>().Update(user);
            await _unitOfWork.SaveChanges();

            // Now it's safe to delete
            _unitOfWork.Repository<Domain.Entities.RefreshToken>().Delete(tokenIdToDelete);
            await _unitOfWork.SaveChanges();
        }

        // Add new token
        _unitOfWork.Repository<Domain.Entities.RefreshToken>().Add(newRefreshToken);
        await _unitOfWork.SaveChanges(); // Save to get the new ID

        // Now link it to the user with both navigation property and ID
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenId = newRefreshToken.Id.ToString(); // Set the string ID
        await _unitOfWork.Repository<User>().Update(user);
        await _unitOfWork.SaveChanges();

        _logger.LogInformation("User {UserId} logged in successfully", user.Id);

        // Create the LoginResponse with nested Tokens and User information
        return GenericResponseModel<LoginResponse>.Success(new LoginResponse
        {
            Tokens = new AuthResponse
            {
                Token = newAccesstoken,
                RefreshToken = newRefreshToken.Token,
                ExpiresAt = _tokenService.GetTokenExpiration()
            },
            User = new LoginResponse.UserInfo
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed
            }
        });
    }
}