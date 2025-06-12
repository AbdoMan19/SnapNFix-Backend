using Mapster;
using MapsterMapper;
using MediatR;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.SnapReport.DTOs;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Domain.Enums;
using SnapNFix.Application.Features.Issue.Queries;

namespace SnapNFix.Application.Features.SnapReport.Queries;

public class GetSnapReportsByIssueIdQueryHandler : 
    IRequestHandler<GetSnapReportsByIssueIdQuery, GenericResponseModel<PagedList<ReportDetailsDto>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    private readonly IUserService _userService;

    public GetSnapReportsByIssueIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork, IUserService userService)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
        _userService = userService;
    }


  public async Task<GenericResponseModel<PagedList<ReportDetailsDto>>> Handle(
      GetSnapReportsByIssueIdQuery request, CancellationToken cancellationToken)
  {
      var query = _unitOfWork.Repository<Domain.Entities.SnapReport>()
        .GetQuerableData()
        .Where(r => r.IssueId == request.Id);

      query = query.OrderByDescending(r => r.CreatedAt);

    var reports = await PagedList<Domain.Entities.SnapReport>.CreateAsync(
        query,
        request.PageNumber,
        10,
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