using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Interfaces;
using SnapNFix.Application.Utilities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Auth.Logout;

public class LogoutCommandHandler(ITokenService tokenService  , IUnitOfWork unitOfWork , IHttpContextAccessor httpContextAccessor) 
    : IRequestHandler<LogoutCommand, GenericResponseModel<bool>>
{
    public async Task<GenericResponseModel<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    { 
        var currentUserId = httpContextAccessor?.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
       if (!Guid.TryParse(currentUserId, out var userId))
       {
           return GenericResponseModel<bool>.Failure(Constants.FailureMessage , 
               new List<ErrorResponseModel> { new() { Message = "Invalid user ID" } });
       }

       await unitOfWork.Repository<Domain.Entities.RefreshToken>()
           .DeleteAll(r => r.UserId == userId);
       await unitOfWork.SaveChanges();
       
       return GenericResponseModel<bool>.Success(true);
        
        
    }
}