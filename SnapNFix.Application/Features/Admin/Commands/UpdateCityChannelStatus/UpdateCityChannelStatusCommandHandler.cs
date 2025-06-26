using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Admin.Commands.UpdateCityChannelStatus
{
    public class UpdateCityChannelStatusCommandHandler : IRequestHandler<UpdateCityChannelStatusCommand, GenericResponseModel<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateCityChannelStatusCommandHandler> _logger;

        public UpdateCityChannelStatusCommandHandler(IUnitOfWork unitOfWork, ILogger<UpdateCityChannelStatusCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<GenericResponseModel<bool>> Handle(
            UpdateCityChannelStatusCommand request, CancellationToken cancellationToken)
        {
            var cityRepo = _unitOfWork.Repository<CityChannel>();

            var city = await cityRepo.FindBy(c => c.Id == request.CityId)
                .FirstOrDefaultAsync(cancellationToken);
            if (city == null)
            {
                return GenericResponseModel<bool>.Failure(
                    "City not found",
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create(nameof(request.CityId), "City with this ID does not exist")
                    });
            }

            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                city.IsActive = request.IsActive;

                await cityRepo.Update(city);
                await _unitOfWork.SaveChanges();
                
                await transaction.CommitAsync(cancellationToken);
                _logger.LogInformation("City {CityId} status updated to {IsActive}", request.CityId, request.IsActive);
                
                return GenericResponseModel<bool>.Success(true);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Failed to update city {CityId} status", request.CityId);
                return GenericResponseModel<bool>.Failure($"Failed to update city status: {ex.Message}",
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create(nameof(request.CityId), "Failed to update city status")
                    });
            }
        }
    }
}