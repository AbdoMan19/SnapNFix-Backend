using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Interfaces;
using SnapNFix.Application.Resources;
using SnapNFix.Application.Utilities;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Auth.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, GenericResponseModel<AuthResponse>>
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
        try
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            try
            {
                var refreshToken = await _unitOfWork.Repository<Domain.Entities.RefreshToken>()
                    .FindBy(x => x.Token == request.RefreshToken)
                    .Include(r => r.UserDevice)
                    .ThenInclude(u => u.User)
                    .FirstOrDefaultAsync(cancellationToken);
                
                if (refreshToken is null || refreshToken.IsExpired)
                {
                    _logger.LogWarning("Invalid refresh token attempt: {TokenPrefix}...", 
                        request.RefreshToken.Substring(0, Math.Min(6, request.RefreshToken.Length)));
                        
                    return GenericResponseModel<AuthResponse>.Failure(
                        Constants.FailureMessage, 
                        new List<ErrorResponseModel>
                        {
                            ErrorResponseModel.Create("RefreshToken", Shared.InvalidRefreshToken)
                        });
                }

                var device = refreshToken.UserDevice;
                var user = device.User;
                
                device.LastUsedAt = DateTime.UtcNow;
                await _unitOfWork.Repository<UserDevice>().Update(device);
                
                var (newAccessToken, newRefreshToken) = await _tokenService.RefreshTokenAsync(refreshToken);
                refreshToken.Token = newRefreshToken;
                refreshToken.Expires = _tokenService.GetRefreshTokenExpirationDays();
                await _unitOfWork.Repository<Domain.Entities.RefreshToken>().Update(refreshToken);

                await _unitOfWork.SaveChanges();
                await transaction.CommitAsync(cancellationToken);
                
                _logger.LogInformation("Token refreshed for user {UserId} on device {DeviceId}", 
                    user.Id, device.DeviceId);
                    
                return GenericResponseModel<AuthResponse>.Success(new AuthResponse
                {
                    Token = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = _tokenService.GetTokenExpiration()
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error refreshing token");
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception during token refresh");
            return GenericResponseModel<AuthResponse>.Failure(Shared.OperationFailed);
        }
    }
}

