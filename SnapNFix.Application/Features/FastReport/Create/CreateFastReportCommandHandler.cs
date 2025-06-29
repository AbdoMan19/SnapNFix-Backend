using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.FastReport.Create;

public class CreateFastReportCommandHandler : IRequestHandler<CreateFastReportCommand, GenericResponseModel<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;
    private readonly ILogger<CreateFastReportCommandHandler> _logger;

    public CreateFastReportCommandHandler(IUnitOfWork unitOfWork, IUserService userService, ILogger<CreateFastReportCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _userService = userService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<bool>> Handle(CreateFastReportCommand request, CancellationToken cancellationToken)
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        
        var issueExists = await _unitOfWork.Repository<Domain.Entities.Issue>()
            .FindBy(i => i.Id == request.IssueId)
            .AnyAsync(cancellationToken);

        if (!issueExists)
        {
            _logger.LogWarning("Fast report creation failed: Issue {IssueId} not found", request.IssueId);
            return GenericResponseModel<bool>.Failure(Shared.IssueNotFound,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create(nameof(request.IssueId), Shared.IssueNotFound)
                });
        }

        var existingFastReport = await _unitOfWork.Repository<Domain.Entities.FastReport>()
            .FindBy(fr => fr.UserId == userId && fr.IssueId == request.IssueId)
            .AnyAsync(cancellationToken);

        if (existingFastReport)
        {
            _logger.LogWarning("Fast report creation failed: User {UserId} already created a fast report for issue {IssueId}", 
                userId, request.IssueId);
            return GenericResponseModel<bool>.Failure(Shared.FastReportAlreadyExists,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create(nameof(request.IssueId), Shared.FastReportAlreadyExists)
                });
        }

        var existingSnapReport = await _unitOfWork.Repository<Domain.Entities.SnapReport>()
            .FindBy(sr => sr.UserId == userId && sr.IssueId == request.IssueId)
            .AnyAsync(cancellationToken);

        if (existingSnapReport)
        {
            _logger.LogWarning("Fast report creation failed: User {UserId} already created a snap report for issue {IssueId}", 
                userId, request.IssueId);
            return GenericResponseModel<bool>.Failure(Shared.CannotCreateFastReportForOwnSnapReport,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create(nameof(request.IssueId), Shared.CannotCreateFastReportForOwnSnapReport)
                });
        }

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            var fastReport = new Domain.Entities.FastReport
            {
                UserId = userId,
                IssueId = request.IssueId,
                Comment = request.Comment,
                Severity = request.Severity,
            };

            await _unitOfWork.Repository<Domain.Entities.FastReport>().Add(fastReport);
            await _unitOfWork.SaveChanges();
            await transaction.CommitAsync(cancellationToken);
            
            _logger.LogInformation("Fast report created successfully for IssueId: {IssueId} by UserId: {UserId}", 
                request.IssueId, userId);
            return GenericResponseModel<bool>.Success(true);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "An error occurred while creating the fast report for IssueId: {IssueId}", request.IssueId);
            return GenericResponseModel<bool>.Failure(Shared.FailedToCreateFastReport,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create("Exception", ex.Message)
                });
        }
    }
}