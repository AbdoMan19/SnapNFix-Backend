using MediatR;
using Microsoft.IdentityModel.Tokens;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler(ITokenService tokenService) 
    : IRequestHandler<RefreshTokenCommand, GenericResponseModel<AuthResponse>>
{
    public async Task<GenericResponseModel<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var (newAccessToken, newRefreshToken) = await tokenService.RefreshTokenAsync(
                request.AccessToken, 
                request.RefreshToken, 
                request.IpAddress);
            
            return GenericResponseModel<AuthResponse>.Success(new AuthResponse
            {
                Token = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = tokenService.GetTokenExpiration()
            });
        }
        catch (SecurityTokenException ex)
        {
            return GenericResponseModel<AuthResponse>.Failure(
                "Invalid token", 
                [ErrorResponseModel.Create("Token", ex.Message)]);
        }
        catch (Exception ex)
        {
            return GenericResponseModel<AuthResponse>.Failure(
                "Failed to refresh token", 
                [ErrorResponseModel.Create("Token", ex.Message)]);
        }
    }
}