﻿using MediatR;
using Microsoft.Extensions.Logging;
using SnapNFix.Application.Common.ResponseModel;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Domain.Entities;
using Mapster;
using SnapNFix.Application.Options;

namespace SnapNFix.Application.Features.Statistics.Queries.GetMonthlyTarget;

public class GetMonthlyTargetQueryHandler : IRequestHandler<GetMonthlyTargetQuery, GenericResponseModel<MonthlyTargetDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetMonthlyTargetQueryHandler> _logger;

    public GetMonthlyTargetQueryHandler(
        IUnitOfWork unitOfWork,
        ICacheService cacheService,
        ILogger<GetMonthlyTargetQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<GenericResponseModel<MonthlyTargetDto>> Handle(
        GetMonthlyTargetQuery request, 
        CancellationToken cancellationToken)
    {
        try
        {
            var cached = await _cacheService.GetAsync<MonthlyTargetDto>(CacheKeys.MonthlyTarget);
            if (cached != null)
            {
                return GenericResponseModel<MonthlyTargetDto>.Success(cached);
            }

            // Get current target from database
            var currentTarget = await _unitOfWork.Repository<AdminTarget>()
                .FindBy(t => t.IsActive)
                .OrderByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            var targetResolutionRate = currentTarget?.TargetResolutionRate ?? 95.0;

            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            var monthlyStats = await _unitOfWork.Repository<SnapNFix.Domain.Entities.Issue>()
                .FindBy(i => i.CreatedAt >= startOfMonth)
                .GroupBy(i => 1)
                .Select(g => new
                {
                    Total = g.Count(),
                    Resolved = g.Count(i => i.Status == IssueStatus.Completed)
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (monthlyStats == null || monthlyStats.Total == 0)
            {
                var defaultTarget = new MonthlyTargetDto
                {
                    TargetResolutionRate = targetResolutionRate,
                    CurrentResolutionRate = 0,
                    Progress = 0,
                    IncidentsToTarget = 0,
                    Status = "No Data",
                    Percentage = 0,
                    Target = targetResolutionRate,
                    Current = 0,
                    Improvement = 0
                };

                await _cacheService.SetAsync(CacheKeys.MonthlyTarget, defaultTarget, TimeSpan.FromMinutes(5));
                return GenericResponseModel<MonthlyTargetDto>.Success(defaultTarget);
            }

            var currentResolutionRate = Math.Round((double)monthlyStats.Resolved / monthlyStats.Total * 100, 2);
            var progress = Math.Round(currentResolutionRate / targetResolutionRate * 100, 2);
            var incidentsToTarget = Math.Max(0, (int)Math.Ceiling(monthlyStats.Total * (targetResolutionRate / 100)) - monthlyStats.Resolved);

            var status = currentResolutionRate >= targetResolutionRate ? "Ahead" :
                        currentResolutionRate >= targetResolutionRate * 0.9 ? "On Track" : "Behind";

            var target = new MonthlyTargetDto
            {
                TargetResolutionRate = targetResolutionRate,
                CurrentResolutionRate = currentResolutionRate,
                Progress = Math.Min(progress, 100),
                IncidentsToTarget = incidentsToTarget,
                Status = status,
                Percentage = currentResolutionRate,
                Target = targetResolutionRate,
                Current = currentResolutionRate,
                Improvement = Math.Round(currentResolutionRate - targetResolutionRate, 2)
            };

            await _cacheService.SetAsync(CacheKeys.MonthlyTarget, target, TimeSpan.FromMinutes(5));
            
            return GenericResponseModel<MonthlyTargetDto>.Success(target);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving monthly target");
            return GenericResponseModel<MonthlyTargetDto>.Failure(Shared.OperationFailed,
                new List<ErrorResponseModel> { new ErrorResponseModel { Message = ex.Message } });
        }
    }
}