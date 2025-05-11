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

    public async Task<GenericResponseModel<bool>> Handle(ImageValidationResultCommand request,
        CancellationToken cancellationToken)
    {
        try
        {
            var report = await _unitOfWork.Repository<Domain.Entities.SnapReport>()
                .FindBy(r => r.TaskId == request.TaskId)
                .FirstOrDefaultAsync(cancellationToken);

            if (report == null)
            {
                return GenericResponseModel<bool>.Failure("Report not found");
            }

            report.ImageStatus = request.ImageStatus;
            report.Category = request.ReportCategory;
            //report.Threshold = request.Threshold;

            if (report.ImageStatus == ImageStatus.Approved)
            {
                await _reportService.AttachReportWithIssue(report, cancellationToken);
            }else

            await _unitOfWork.Repository<Domain.Entities.SnapReport>().Update(report);
            await _unitOfWork.SaveChanges();
            return GenericResponseModel<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing AI validation result");
            return GenericResponseModel<bool>.Failure("Failed to process validation result");
        }
    }
}