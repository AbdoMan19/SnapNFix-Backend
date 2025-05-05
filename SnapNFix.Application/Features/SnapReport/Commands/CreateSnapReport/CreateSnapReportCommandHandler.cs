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
    public CreateSnapReportCommandHandler(IUnitOfWork unitOfWork, ILogger<CreateSnapReportCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<GenericResponseModel<ReportDetailsDto>> Handle(CreateSnapReportCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Create point geometry from coordinates
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var reportLocation = geometryFactory.CreatePoint(new Coordinate(request.Longitude, request.Latitude));

            // Search for nearby issues with same category
            var nearbyIssue = await FindNearbyIssue(reportLocation, request.Category , cancellationToken);

            // Create the SnapReport
            var snapReport = new Domain.Entities.SnapReport
            {
                Comment = request.Comment,
                ImagePath = request.ImagePath,
                Location = reportLocation,
                Category = request.Category,
                UserId = request.UserId,
            };

            if (nearbyIssue != null)
            {
                // Assign to existing issue
                snapReport.IssueId = nearbyIssue.Id;
            }
            else
            {
                // Create new issue
                var issue = new Issue();
                await _unitOfWork.Repository<Issue>().Add(issue);
                /*await _unitOfWork.SaveChanges();*/
                
                snapReport.IssueId = issue.Id;
                snapReport.Issue = issue;

            }

            await _unitOfWork.Repository<Domain.Entities.SnapReport>().Add(snapReport);
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

    private async Task<Issue?> FindNearbyIssue(Point location, ReportCategory category , CancellationToken cancellationToken)
    {
        var nearbyIssue = await _unitOfWork.Repository<Issue>()
            .FindBy(i => i.Category == category && i.Location.IsWithinDistance(location, Constants.NearbyIssueRadiusMeters))
            .OrderBy(i => i.Location.Distance(location))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken); 
        return nearbyIssue;
    }
}