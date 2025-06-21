using Application.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Ai.AiValidation.ReportImageProcessing;

public class ImageValidationResultCommandHandler : IRequestHandler<ImageValidationResultCommand, GenericResponseModel<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ImageValidationResultCommandHandler> _logger;
    private readonly IReportService _reportService;
    private readonly IMediator _mediator;

    public ImageValidationResultCommandHandler(
        IUnitOfWork unitOfWork,
        IReportService reportService,
        ILogger<ImageValidationResultCommandHandler> logger, 
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _reportService = reportService;
        _mediator = mediator;
    }

    public async Task<GenericResponseModel<bool>> Handle(
        ImageValidationResultCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing AI validation result for TaskId: {TaskId}, Status: {Status}, Category: {Category}, Threshold: {Threshold}",
            request.TaskId, request.ImageStatus, request.ReportCategory, request.Threshold);

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        Domain.Entities.SnapReport? report = null;
        try
        {
            report = await _unitOfWork.Repository<Domain.Entities.SnapReport>()
                .FindBy(r => r.TaskId == request.TaskId)
                .FirstOrDefaultAsync(cancellationToken);

            if (report == null)
            {
                _logger.LogWarning("Report not found for TaskId: {TaskId}", request.TaskId);
                return GenericResponseModel<bool>.Failure("Report not found for the given TaskId");
            }

            _logger.LogInformation("Found report {ReportId} for TaskId: {TaskId}. Current status: {CurrentStatus}",
                report.Id, request.TaskId, report.ImageStatus);

            if (report.ImageStatus != ImageStatus.Pending)
            {
                _logger.LogWarning("Report {ReportId} with TaskId: {TaskId} is not in pending status. Current status: {CurrentStatus}",
                    report.Id, request.TaskId, report.ImageStatus);
                return GenericResponseModel<bool>.Failure("Report is not in pending status and cannot be updated");
            }

            var oldStatus = report.ImageStatus;
            var oldCategory = report.ReportCategory;

            report.ImageStatus = request.ImageStatus;
            report.ReportCategory = request.ReportCategory;
            report.Threshold = request.Threshold;

            await _unitOfWork.Repository<Domain.Entities.SnapReport>().Update(report);

            _logger.LogInformation("Updated report {ReportId}: Status {OldStatus} -> {NewStatus}, Category {OldCategory} -> {NewCategory}, Threshold: {Threshold}",
                report.Id, oldStatus, request.ImageStatus, oldCategory, request.ReportCategory, request.Threshold);

            if (report.ImageStatus == ImageStatus.Approved)
            {
                _logger.LogInformation("Report {ReportId} approved, attempting to attach to issue", report.Id);

                try
                {
                    bool foundNearbyIssue = await _reportService.AttachReportWithNearbyIssue(report, cancellationToken);
                    if (foundNearbyIssue)
                    {
                        _logger.LogInformation("Successfully attached report {ReportId} to issue {IssueId}",
                            report.Id, report.IssueId);
                    }else
                    {
                        _logger.LogInformation("No nearby issue found for report {ReportId}, creating new issue", report.Id);
                        var newIssue = await _reportService.CreateIssueWithReportAsync(report, cancellationToken);
                        _logger.LogInformation("Created new issue for report {ReportId} with IssueId {IssueId}",
                            report.Id, report.IssueId);
                        await _mediator.Publish(new IssueCreated(newIssue), cancellationToken);
                    }
                    
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error attaching report {ReportId} to issue", report.Id);
                }
            }
            else if (report.ImageStatus == ImageStatus.Declined)
            {
                _logger.LogInformation("Report {ReportId} declined by AI validation", report.Id);
            }

            await _unitOfWork.SaveChanges();
            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Successfully processed AI validation result for TaskId: {TaskId}, Report: {ReportId}",
                request.TaskId, report.Id);
            
            await _mediator.Publish(new SnapReportStatusChanged(report , oldStatus , request.ImageStatus), cancellationToken);
 

            return GenericResponseModel<bool>.Success(true);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while processing AI validation result for TaskId: {TaskId}", request.TaskId);
            return GenericResponseModel<bool>.Failure("The report was modified by another process. Please try again.");
        }
        catch (DbUpdateException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Database error while processing AI validation result for TaskId: {TaskId}", request.TaskId);

            try
            {
                if (report != null)
                {
                    report.ImageStatus = ImageStatus.Declined;
                    report.IssueId = null;
                    report.Issue = null; 
                    await _unitOfWork.Repository<Domain.Entities.SnapReport>().Update(report);
                    await _unitOfWork.SaveChanges();
                    _logger.LogInformation("Set report {ReportId} status to Declined due to DbUpdateException", report.Id);
                }
            }
            catch (Exception updateEx)
            {
                _logger.LogError(updateEx, "Failed to set report status to Declined after DbUpdateException for TaskId: {TaskId}", request.TaskId);
            }

            return GenericResponseModel<bool>.Failure("Database error occurred while updating the report");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Unexpected error processing AI validation result for TaskId: {TaskId}", request.TaskId);
            return GenericResponseModel<bool>.Failure("An unexpected error occurred while processing the validation result");
        }
    }
}

