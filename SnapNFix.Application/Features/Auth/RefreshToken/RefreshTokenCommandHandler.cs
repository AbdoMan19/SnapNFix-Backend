using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Utilities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler
    : IRequestHandler<RefreshTokenCommand, GenericResponseModel<AuthResponse>>
{
    private readonly ILogger<RefreshTokenCommandHandler> _logger;
    private readonly ITokenService _tokenService;
    private readonly IUnitOfWork _unitOfWork;
    public RefreshTokenCommandHandler(
        IUnitOfWork unitOfWork, 
        ITokenService tokenService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _logger = logger;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
    }
    
    public async Task<GenericResponseModel<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshToken = await _unitOfWork.Repository<Domain.Entities.RefreshToken>()
            .FindBy(x => x.Token == request.RefreshToken)
            .Include(r => r.UserDevice)
            .ThenInclude(u => u.User)
            .FirstOrDefaultAsync(cancellationToken);
        
        if(refreshToken is null || refreshToken.IsExpired)
        {
            _logger.LogWarning("Invalid refresh token attempt: {Token}", request.RefreshToken);
            return GenericResponseModel<AuthResponse>.Failure(Constants.FailureMessage, new List<ErrorResponseModel>
            {
                ErrorResponseModel.Create("RefreshToken", "Invalid or expired refresh token")
            });
        }
        var (newAccessToken, newRefreshToken) = await _tokenService.RefreshTokenAsync(
            refreshToken);
        
        _logger.LogInformation("Token refreshed for user {UserId}", refreshToken.UserDevice.UserId);
        return GenericResponseModel<AuthResponse>.Success(new AuthResponse
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = _tokenService.GetTokenExpiration()
        });

    }
}