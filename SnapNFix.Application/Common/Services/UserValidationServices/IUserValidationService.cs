using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Entities;

namespace SnapNFix.Application.Common.Services;

public interface IUserValidationService
{
    Task<(User user, GenericResponseModel<T>? ErrorResponse)> ValidateUserAsync<T>(string emailOrPhone);
}