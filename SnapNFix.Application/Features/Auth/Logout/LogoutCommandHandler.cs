using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Utilities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Auth.Logout;

public class LogoutCommandHandler
    : IRequestHandler<LogoutCommand, GenericResponseModel<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    private readonly ILogger<LogoutCommandHandler> _logger;
    private readonly IDeviceManager _deviceManager;

    public LogoutCommandHandler(IUnitOfWork unitOfWork, IDeviceManager deviceManager, ILogger<LogoutCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _deviceManager = deviceManager;
    }

    public async Task<GenericResponseModel<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var currentDeviceId = _deviceManager.GetCurrentDeviceId();
       if (currentDeviceId == null)
       {
           _logger.LogWarning("Logout attempt without a valid device ID");
           return GenericResponseModel<bool>.Failure(Constants.FailureMessage , 
               new List<ErrorResponseModel> {ErrorResponseModel.Create("DeviceId", "Invalid Device Id")});
       }

       var refreshToken = await _unitOfWork.Repository<Domain.Entities.RefreshToken>()
           .FindBy(r => r.UserDeviceId == currentDeviceId && r.IsActive)
           .FirstOrDefaultAsync(cancellationToken);
       if (refreshToken != null)
       {
           refreshToken.Revoked = DateTime.UtcNow;
           await _unitOfWork.Repository<Domain.Entities.RefreshToken>().Update(refreshToken);
           await _unitOfWork.SaveChanges();
       }
       else
       {
              _logger.LogWarning("No active refresh token found for device {DeviceId}", currentDeviceId);
       }
       
       return GenericResponseModel<bool>.Success(true);
       

        
        
    }
}