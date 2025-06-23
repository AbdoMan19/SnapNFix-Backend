using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Interfaces;
using SnapNFix.Application.Resources;
using SnapNFix.Application.Utilities;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Common.Services.UserValidationServices;

public class UserValidationService : IUserValidationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserValidationService> _logger;
    private readonly IUserService _userService;
    
    
    public UserValidationService(IUnitOfWork unitOfWork, ILogger<UserValidationService> logger , IUserService userService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _userService = userService;
    }

    public async Task<(User user, GenericResponseModel<T>? ErrorResponse)> ValidateUserAsync<T>(
        string emailOrPhone)
    {
        var user = await _unitOfWork.Repository<User>()
            .FindBy(u => (u.Email == emailOrPhone || u.PhoneNumber == emailOrPhone))
            .FirstOrDefaultAsync();

        if (user == null || user.IsDeleted)
        {
            _logger.LogWarning("Validation failed: User not found for identifier {Identifier}", emailOrPhone);
            return (null, GenericResponseModel<T>.Failure(
                Constants.FailureMessage,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create("EmailOrPhone", Shared.UserNotFound)
                }));
        }

        if (user.IsSuspended)
        {
            _logger.LogWarning("Validation failed: User is suspended for identifier {Identifier}", emailOrPhone);
            return (null, GenericResponseModel<T>.Failure(
                Constants.FailureMessage,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create("EmailOrPhone", Shared.AccountSuspended)
                }));
        }

        return (user, null);
    }
}

