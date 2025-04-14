using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Auth.Logout;

public class LogoutCommandHandler(ITokenService tokenService) 
    : IRequestHandler<LogoutCommand, GenericResponseModel<bool>>
{
    public async Task<GenericResponseModel<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await tokenService.RevokeRefreshTokenAsync(request.RefreshToken, request.IpAddress);
            
            return GenericResponseModel<bool>.Success(true);
        }
        catch (Exception ex)
        {
            return GenericResponseModel<bool>.Failure(
                "Failed to logout", 
                new List<ErrorResponseModel> { ErrorResponseModel.Create("Token", ex.Message) });
        }
    }
}