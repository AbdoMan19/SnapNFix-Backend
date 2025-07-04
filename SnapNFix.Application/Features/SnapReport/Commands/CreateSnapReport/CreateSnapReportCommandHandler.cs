using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Common.Services.LocationValidation;
using SnapNFix.Application.Features.SnapReport.DTOs;
using SnapNFix.Application.Interfaces;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.SnapReport.Commands.CreateSnapReport;

public class CreateSnapReportCommandHandler : IRequestHandler<CreateSnapReportCommand, GenericResponseModel<ReportDetailsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateSnapReportCommandHandler> _logger;
    private readonly IUserService _userService;
    private readonly IPhotoValidationService _photoValidationService;
    private readonly IImageProcessingService _imageProcessingService;
    private readonly ILocationValidationService _locationValidationService;
    private readonly ICacheInvalidationService _cacheInvalidationService;

    public CreateSnapReportCommandHandler(
        IUnitOfWork unitOfWork,
        IUserService userService,
        IPhotoValidationService photoValidationService,
        IImageProcessingService imageProcessingService,
        ILocationValidationService locationValidationService,
        ILogger<CreateSnapReportCommandHandler> logger,
        ICacheInvalidationService cacheInvalidationService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _userService = userService;
        _photoValidationService = photoValidationService;
        _imageProcessingService = imageProcessingService;
        _locationValidationService = locationValidationService;
        _cacheInvalidationService = cacheInvalidationService;
    }

    public async Task<GenericResponseModel<ReportDetailsDto>> Handle(CreateSnapReportCommand request, CancellationToken cancellationToken)
    {
        if (!_locationValidationService.IsWithinEgypt(request.Latitude, request.Longitude))
        {
            var locationMessage = _locationValidationService.GetLocationValidationMessage(request.Latitude, request.Longitude);
            _logger.LogWarning("Report creation denied for location outside Egypt: Lat={Latitude}, Lng={Longitude}", 
                request.Latitude, request.Longitude);
            
            return GenericResponseModel<ReportDetailsDto>.Failure(locationMessage, new List<ErrorResponseModel>
            {
                ErrorResponseModel.Create("Location", locationMessage)
            });
        }

        var (isValid, errorMessage) = await _imageProcessingService.ValidateImageAsync(request.Image);
        if (!isValid)
        {
            _logger.LogWarning("Image validation failed: {ErrorMessage}", errorMessage);
            return GenericResponseModel<ReportDetailsDto>.Failure(errorMessage,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create("Image", errorMessage)
                });
        }

        try
        {
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            try
            {
                var currentUserId = await _userService.GetCurrentUserIdAsync();
                var imagePath = await _imageProcessingService.SaveImageAsync(request.Image, "snapreports");
                var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
                var point = geometryFactory.CreatePoint(new Coordinate(request.Longitude, request.Latitude));
                
                var snapReport = new Domain.Entities.SnapReport
                {
                    Comment = request.Comment,
                    ImagePath = imagePath,
                    Location = point,
                    UserId = currentUserId,
                    ImageStatus = ImageStatus.Pending,
                    ReportCategory = ReportCategory.NotSpecified,
                    Severity = request.Severity,
                    Road = request.Road,
                    City = request.City,
                    State = request.State,
                    Country = request.Country
                };

                await _unitOfWork.Repository<Domain.Entities.SnapReport>().Add(snapReport);
                await _unitOfWork.SaveChanges();
                await transaction.CommitAsync(cancellationToken);
                
                await _cacheInvalidationService.InvalidateUserCacheAsync(currentUserId);
                await _cacheInvalidationService.InvalidateStatisticsCacheAsync();
                
                _photoValidationService.ProcessPhotoValidationInBackgroundAsync(snapReport);
                
                _logger.LogInformation("Successfully created snap report with ID {ReportId} for user {UserId} at location Lat={Latitude}, Lng={Longitude}", 
                    snapReport.Id, currentUserId, request.Latitude, request.Longitude);

                var reportDto = snapReport.Adapt<ReportDetailsDto>();
                return GenericResponseModel<ReportDetailsDto>.Success(reportDto);
            }
            catch (DbUpdateException dbEx)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(dbEx, "Database error creating snap report");
                return GenericResponseModel<ReportDetailsDto>.Failure(Shared.OperationFailed,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create("Database", Shared.OperationFailed)
                    });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error during snap report creation transaction");
                return GenericResponseModel<ReportDetailsDto>.Failure(Shared.OperationFailed,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create("Transaction", Shared.OperationFailed)
                    });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception creating snap report");
            return GenericResponseModel<ReportDetailsDto>.Failure(Shared.OperationFailed,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create("Unhandled", Shared.OperationFailed)
                });
        }
    }
}

