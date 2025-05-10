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

    public CreateSnapReportCommandHandler(
        IUnitOfWork unitOfWork, 
        IUserService userService,
        IPhotoValidationService photoValidationService,
        ILogger<CreateSnapReportCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _userService = userService;
        photoValidationService = _photoValidationService;
    }

    public async Task<GenericResponseModel<ReportDetailsDto>> Handle(CreateSnapReportCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var currentuserId = await _userService.GetCurrentUserIdAsync();
            if (currentuserId == Guid.Empty)
            {
                return GenericResponseModel<ReportDetailsDto>.Failure("User not found");
            }

            // Create initial report
            var snapReport = new Domain.Entities.SnapReport
            {
                Comment = request.Comment,
                ImagePath = request.ImagePath,
                Location = geometryFactory.CreatePoint(new Coordinate(request.Longitude, request.Latitude)),
                UserId = currentuserId,
                ImageStatus = ImageStatus.Pending,
                Category = ReportCategory.NotSpecified
            };

            await _unitOfWork.Repository<Domain.Entities.SnapReport>().Add(snapReport);
            await _unitOfWork.SaveChanges();

            // Send to AI service
            var taskId = await _photoValidationService.SendImageForValidationAsync(
                snapReport.ImagePath,
                cancellationToken);
            _logger.LogInformation("Image of report {ReportId} sent for validation. TaskId: {TaskId}", taskId , snapReport.Id);

            snapReport.TaskId = taskId;
            await _unitOfWork.SaveChanges();

            var reportDto = snapReport.Adapt<ReportDetailsDto>();
            return GenericResponseModel<ReportDetailsDto>.Success(reportDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating snap report");
            return GenericResponseModel<ReportDetailsDto>.Failure("Failed to create report");
        }
    }
    
}