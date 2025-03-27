using Google.Apis.Auth;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Utilities;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Auth.GoogleLogin;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, GenericResponseModel<AuthResponse>>
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public GoogleLoginCommandHandler(
        UserManager<User> userManager,
        ITokenService tokenService,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<GenericResponseModel<AuthResponse>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _configuration["Authentication:Google:ClientId"] }
        };
        var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
        
        var user = await _userManager.FindByEmailAsync(payload.Email);
        if (user == null)
        {
            user = new User
            {
                Email = payload.Email,
                FirstName = payload.GivenName,
                LastName = payload.FamilyName,
                EmailConfirmed = payload.EmailVerified,
                PhoneNumberConfirmed = false,
            };
            await _userManager.AddToRoleAsync(user , "Citizen");
            var result = await _userManager.CreateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => ErrorResponseModel.Create(e.Code , e.Description)).ToList();
                return GenericResponseModel<AuthResponse>.Failure(Constants.FailureMessage, errors);

            }
        }
        
        var token = await _tokenService.GenerateJwtToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        return GenericResponseModel<AuthResponse>.Success(new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = _tokenService.GetTokenExpiration()
        });

    }
}