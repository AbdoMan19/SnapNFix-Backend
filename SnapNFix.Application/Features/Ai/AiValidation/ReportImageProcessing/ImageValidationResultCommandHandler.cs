using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Ai.AiValidation.ReportImageProcessing;

public class ImageValidationResultCommandHandler : IRequestHandler<ImageValidationResultCommand , GenericResponseModel<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ImageValidationResultCommandHandler> _logger;
    private readonly IReportService _reportService;

    public ImageValidationResultCommandHandler(
        IUnitOfWork unitOfWork,
        IReportService reportService,
        ILogger<ImageValidationResultCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _reportService = reportService;
        
    }

    public async Task<GenericResponseModel<bool>> Handle(
        ImageValidationResultCommand request,
        CancellationToken cancellationToken)
    {
        // Start a transaction for all operations
        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            // Find the report using TaskId index
            var report = await _unitOfWork.Repository<Domain.Entities.SnapReport>()
                .FindBy(r => r.TaskId == request.TaskId)
                .FirstOrDefaultAsync(cancellationToken);

            if (report == null)
            {
                return GenericResponseModel<bool>.Failure("Report not found");
            }

         
            // Update report properties
            report.ImageStatus = request.ImageStatus;
            report.Category = request.ReportCategory;
            report.Threshold = request.Threshold;
            
            // Only update in database if changes were made
            await _unitOfWork.Repository<Domain.Entities.SnapReport>().Update(report);
            
            // If approved, handle issue attachment within the same transaction
            if (report.ImageStatus == ImageStatus.Approved)
            {
                await _reportService.AttachReportWithIssue(report, cancellationToken);
            }
            
           
    
            
            // Commit all changes in a single transaction
            await _unitOfWork.SaveChanges();
            await transaction.CommitAsync(cancellationToken);
            
            return GenericResponseModel<bool>.Success(true);
        }
        catch (Exception ex)
        {
            // Explicit rollback on failure
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error processing AI validation result for TaskId: {TaskId}", request.TaskId);
            return GenericResponseModel<bool>.Failure("Failed to process validation result");
        }
    }
}