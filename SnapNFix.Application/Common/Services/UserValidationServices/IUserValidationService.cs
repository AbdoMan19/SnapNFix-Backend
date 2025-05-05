using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces.ServiceLifetime;

namespace SnapNFix.Application.Common.Services.UserValidationServices;

public interface IUserValidationService : IScoped
{
    Task<(User user, GenericResponseModel<T>? ErrorResponse)> ValidateUserAsync<T>(string emailOrPhone);
}