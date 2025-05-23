using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.SnapReport.DTOs;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.SnapReport.Queries;

public class GetUserReportsQueryHandler : 
    IRequestHandler<GetUserReportsQuery, GenericResponseModel<PagedList<ReportDetailsDto>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetUserReportsQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<GenericResponseModel<PagedList<ReportDetailsDto>>> Handle(
        GetUserReportsQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Domain.Entities.SnapReport>()
            .GetQuerableData()
            .Where(r => r.UserId == request.UserId);
        
        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(r => r.ImageStatus.ToString() == request.Status);
        }

        if (!string.IsNullOrEmpty(request.Category))
        {
            query = query.Where(r => r.ReportCategory.ToString() == request.Category);
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

        return GenericResponseModel<PagedList<ReportDetailsDto>>.Success(reportDtos);
    }
}