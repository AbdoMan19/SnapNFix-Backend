using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.SnapReport.DTOs;
using SnapNFix.Application.Interfaces;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.SnapReport.Queries.GetUserReportsStatistics;

public class GetUserReportsStatisticsQueryHandler : IRequestHandler<GetUserReportsStatisticsQuery, GenericResponseModel<UserReportsStatisticsDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;
    private readonly ILogger<GetUserReportsStatisticsQueryHandler> _logger;

    public GetUserReportsStatisticsQueryHandler(
        IUnitOfWork unitOfWork, 
        IUserService userService,
        ILogger<GetUserReportsStatisticsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _userService = userService;
        _logger = logger;
    }
    
    public async Task<GenericResponseModel<UserReportsStatisticsDto>> Handle(GetUserReportsStatisticsQuery request, CancellationToken cancellationToken)
    {
        var userId = await _userService.GetCurrentUserIdAsync();
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("Failed to get current user ID");
            return GenericResponseModel<UserReportsStatisticsDto>.Failure(Shared.UserNotFound);
        }
        
        try
        {
            // Get a repository reference once for efficiency
            var reportRepository = _unitOfWork.Repository<Domain.Entities.SnapReport>();
            
            // Count pending reports
            var pendingReports = await reportRepository
                .FindBy(r => r.UserId == userId && r.ImageStatus == ImageStatus.Pending)
                .CountAsync(cancellationToken);
                
            // Count approved reports
            var approvedReports = await reportRepository
                .FindBy(r => r.UserId == userId && r.ImageStatus == ImageStatus.Approved)
                .CountAsync(cancellationToken);

            // Create simplified statistics DTO
            var statistics = new UserReportsStatisticsDto
            {
                PendingReports = pendingReports,
                ApprovedReports = approvedReports
            };
            
            _logger.LogInformation("Retrieved report counts for user {UserId}: {PendingCount} pending, {ApprovedCount} approved", 
                userId, pendingReports, approvedReports);
                
            return GenericResponseModel<UserReportsStatisticsDto>.Success(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving report counts for user {UserId}", userId);
            return GenericResponseModel<UserReportsStatisticsDto>.Failure(Shared.OperationFailed);
        }
    }
}