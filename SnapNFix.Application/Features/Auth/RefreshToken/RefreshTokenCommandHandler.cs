using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler(ITokenService tokenService , IUnitOfWork unitOfWork) 
    : IRequestHandler<RefreshTokenCommand, GenericResponseModel<AuthResponse>>
{
    public async Task<GenericResponseModel<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await unitOfWork.Repository<Domain.Entities.RefreshToken>()
            .FindBy(x => x.Token == request.RefreshToken)
            .Include(r => r.User)
            .FirstOrDefaultAsync(cancellationToken);
        
        if(refreshToken is null || !refreshToken.IsActive)
        {
            return GenericResponseModel<AuthResponse>.Failure("Invalid refresh token", new List<ErrorResponseModel>
            {
                ErrorResponseModel.Create("RefreshToken", "Invalid or expired refresh token")
            });
        }
        var (newAccessToken, newRefreshToken) = await tokenService.RefreshTokenAsync(
            refreshToken);
        return GenericResponseModel<AuthResponse>.Success(new AuthResponse
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = tokenService.GetTokenExpiration()
        });

    }
}