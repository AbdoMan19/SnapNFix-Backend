using Application.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.Extensions;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Interfaces;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Issue.Commands.UpdateIssue;

public class UpdateIssueCommandHandler : IRequestHandler<UpdateIssueCommand, GenericResponseModel<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateIssueCommandHandler> _logger;
    private readonly ICacheInvalidationService _cacheInvalidationService;
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly IMediator _mediator;

    public UpdateIssueCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UpdateIssueCommandHandler> logger,
        ICacheInvalidationService cacheInvalidationService,
        IBackgroundTaskQueue backgroundTaskQueue,
        IMediator mediator)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _cacheInvalidationService = cacheInvalidationService;
        _backgroundTaskQueue = backgroundTaskQueue;
        _mediator = mediator;
    }

    public async Task<GenericResponseModel<bool>> Handle(UpdateIssueCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Processing issue update request for IssueId: {IssueId}", request.Id);

        await using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        
        try
        {
            var issue = await _unitOfWork.Repository<Domain.Entities.Issue>()
                .FindBy(i => i.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

            if (issue == null)
            {
                _logger.LogWarning("Issue not found for IssueId: {IssueId}", request.Id);
                return GenericResponseModel<bool>.Failure(
                    Shared.IssueNotFound,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create(nameof(request.Id), Shared.IssueNotFound)
                    });
            }

            bool hasChanges = false;
            var previousStatus = issue.Status;

            if (request.Status.HasValue && request.Status != issue.Status)
            {
                issue.Status = request.Status.Value;
                hasChanges = true;
                _logger.LogInformation("Issue {IssueId} status changed from {OldStatus} to {NewStatus}", 
                    issue.Id, previousStatus, request.Status.Value);
            }

            if (request.Severity.HasValue && request.Severity != issue.Severity)
            {
                issue.Severity = request.Severity.Value;
                hasChanges = true;
                _logger.LogInformation("Issue {IssueId} severity changed to {NewSeverity}", 
                    issue.Id, request.Severity.Value);
            }


            if (!hasChanges)
            {
                _logger.LogInformation("No changes detected for issue {IssueId}", request.Id);
                return GenericResponseModel<bool>.Success(true);
            }

            await _unitOfWork.Repository<Domain.Entities.Issue>().Update(issue);
            await _unitOfWork.SaveChanges();
            await transaction.CommitAsync(cancellationToken);

            await _cacheInvalidationService.InvalidateIssueCacheAsync(issue.Id);
            await _cacheInvalidationService.InvalidateStatisticsCacheAsync();

            if (request.Status.HasValue && request.Status != previousStatus)
            {
                _backgroundTaskQueue.EnqueueScoped<IMediator>(async mediator => 
                {
                    await mediator.Publish(new IssueStatusChanged(issue.Id, previousStatus, request.Status.Value), 
                        CancellationToken.None);
                });
            }

            _logger.LogInformation("Issue {IssueId} updated successfully", request.Id);
            return GenericResponseModel<bool>.Success(true);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Concurrency conflict while updating issue {IssueId}", request.Id);
            return GenericResponseModel<bool>.Failure(Shared.ConcurrencyConflict);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "Error updating issue {IssueId}", request.Id);
            return GenericResponseModel<bool>.Failure(Shared.IssueUpdateFailed);
        }
    }
}