using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.SnapReport.DTOs;
using SnapNFix.Application.Utilities;
using SnapNFix.Domain.Entities;
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

    public CreateSnapReportCommandHandler(
        IUnitOfWork unitOfWork,
        IUserService userService,
        IPhotoValidationService photoValidationService,
        IImageProcessingService imageProcessingService,
        ILogger<CreateSnapReportCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _userService = userService;
        _photoValidationService = photoValidationService;
        _imageProcessingService = imageProcessingService;
    }

    public async Task<GenericResponseModel<ReportDetailsDto>> Handle(CreateSnapReportCommand request, CancellationToken cancellationToken)
    {
        // Image validation - no database access
        var (isValid, errorMessage) = await _imageProcessingService.ValidateImageAsync(request.Image);
        if (!isValid)
        {
            _logger.LogWarning("Image validation failed: {ErrorMessage}", errorMessage);
            return GenericResponseModel<ReportDetailsDto>.Failure(errorMessage);
        }

        try
        {
            // Start explicit transaction
            await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
            
            try
            {
                // Get current user and prepare data - minimal DB read
                var currentUserId = await _userService.GetCurrentUserIdAsync();
                
                // Save image 
                var imagePath = await _imageProcessingService.SaveImageAsync(request.Image, "snapreports");
                
                // Prepare location data
                var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
                var point = geometryFactory.CreatePoint(new Coordinate(request.Longitude, request.Latitude));
                
                // Create entity
                var snapReport = new Domain.Entities.SnapReport
                {
                    Comment = request.Comment,
                    ImagePath = imagePath,
                    Location = point,
                    UserId = currentUserId,
                    ImageStatus = ImageStatus.Pending,
                    Category = ReportCategory.NotSpecified
                };

                // Database operations - single insert
                await _unitOfWork.Repository<Domain.Entities.SnapReport>().Add(snapReport);
                await _unitOfWork.SaveChanges();
                
                // Commit transaction
                await transaction.CommitAsync(cancellationToken);
                
                // After successful save, start background task
                // Note: This is outside transaction as it's a fire-and-forget operation
                _photoValidationService.ProcessPhotoValidationInBackgroundAsync(snapReport);

                _logger.LogInformation("Successfully created snap report with ID {ReportId} for user {UserId}", 
                    snapReport.Id, currentUserId);
                
                // Map to DTO for response
                var reportDto = new ReportDetailsDto
                {
                    Id = snapReport.Id,
                    Comment = snapReport.Comment,
                    ImagePath = snapReport.ImagePath,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Status = snapReport.ImageStatus,
                    CreatedAt = snapReport.CreatedAt,
                    Category = snapReport.Category,
                    IssueId = snapReport.IssueId
                };
                
                return GenericResponseModel<ReportDetailsDto>.Success(reportDto);
            }
            catch (DbUpdateException dbEx)
            {
                // Database-specific error handling
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(dbEx, "Database error creating snap report");
                return GenericResponseModel<ReportDetailsDto>.Failure("Database error occurred while saving the report");
            }
            catch (Exception ex)
            {
                // General error during transaction
                await transaction.RollbackAsync(cancellationToken);
                _logger.LogError(ex, "Error during snap report creation transaction");
                return GenericResponseModel<ReportDetailsDto>.Failure("Failed to create report");
            }
        }
        catch (Exception ex)
        {
            // Outer exception handling for non-transaction errors
            _logger.LogError(ex, "Unhandled exception creating snap report");
            return GenericResponseModel<ReportDetailsDto>.Failure("An unexpected error occurred");
        }
    }
}