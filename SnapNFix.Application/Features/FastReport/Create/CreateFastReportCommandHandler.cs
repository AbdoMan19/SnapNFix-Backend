using MediatR;
using Microsoft.Extensions.Logging;

using SnapNFix.Domain.Interfaces;

using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.FastReport.Create;

public class CreateFastReportCommandHandler : IRequestHandler<CreateFastReportCommand, GenericResponseModel<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    //user service
    private readonly IUserService _userService;
    //logger
    private readonly ILogger<CreateFastReportCommandHandler> _logger;

    public CreateFastReportCommandHandler(IUnitOfWork unitOfWork, IUserService userService , ILogger<CreateFastReportCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _userService = userService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<bool>> Handle(CreateFastReportCommand request, CancellationToken cancellationToken)
    {
        var userId = await _userService.GetCurrentUserIdAsync();
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
            // Handle domain-specific exceptions
            await transaction.RollbackAsync(cancellationToken);
            _logger.LogError(ex, "An error occurred while creating the fast report for IssueId: {IssueId}", request.IssueId);
            
            return GenericResponseModel<bool>.Failure("An error occurred while creating the fast report.",
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create("Exception", ex.Message)
                });
        }
    }
}