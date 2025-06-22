using Mapster;
using MapsterMapper;
using MediatR;
using SnapNFix.Application.Common.Interfaces;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.SnapReport.DTOs;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.SnapReport.Queries;

public class GetUserReportsQueryHandler : 
    IRequestHandler<GetUserReportsQuery, GenericResponseModel<PagedList<ReportDetailsDto>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserService _userService;
    private readonly ICacheService _cacheService;

    public GetUserReportsQueryHandler(IMapper mapper, IUnitOfWork unitOfWork, IUserService userService, ICacheService cacheService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _userService = userService;
        _cacheService = cacheService;
    }

    public async Task<GenericResponseModel<PagedList<ReportDetailsDto>>> Handle(
        GetUserReportsQuery request, CancellationToken cancellationToken)
    {
        var user = await _userService.GetCurrentUserAsync();
        var cacheKey = CacheKeys.UserReports(user.Id, request.PageNumber, request.Status);

        // Try cache first
        var cached = await _cacheService.GetAsync<PagedList<ReportDetailsDto>>(cacheKey);
        if (cached != null)
        {
            return GenericResponseModel<PagedList<ReportDetailsDto>>.Success(cached);
        }

        var query = _unitOfWork.Repository<Domain.Entities.SnapReport>()
            .GetQuerableData()
            .Where(r => r.UserId == user.Id);

        if (!string.IsNullOrEmpty(request.Status))
        {
            if (Enum.TryParse<ImageStatus>(request.Status, true, out var imageStatus))
            {
                query = query.Where(r => r.ImageStatus == imageStatus);
            }
        }

        if (!string.IsNullOrEmpty(request.Category))
        {
            if (Enum.TryParse<ReportCategory>(request.Category, true, out var reportCategory))
            {
                query = query.Where(r => r.ReportCategory == reportCategory);
            }
        }

        query = query.OrderByDescending(r => r.CreatedAt);

        var reports = await PagedList<Domain.Entities.SnapReport>.CreateAsync(
            query, 
            request.PageNumber, 
            request.PageSize,
            cancellationToken);

        var mappedItems = _mapper.Map<List<ReportDetailsDto>>(reports.Items);
        
        var reportDtos = new PagedList<ReportDetailsDto>(
            mappedItems,
            reports.TotalCount,
            reports.PageNumber,
            reports.PageSize);

        // Cache the result
        await _cacheService.SetAsync(cacheKey, reportDtos, TimeSpan.FromMinutes(10));

        return GenericResponseModel<PagedList<ReportDetailsDto>>.Success(reportDtos);
    }
}