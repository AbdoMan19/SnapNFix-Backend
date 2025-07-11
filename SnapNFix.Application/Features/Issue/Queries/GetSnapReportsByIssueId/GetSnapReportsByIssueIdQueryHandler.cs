using Mapster;
using MapsterMapper;
using MediatR;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.SnapReport.DTOs;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Application.Features.Issue.Queries;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Interfaces;
using SnapNFix.Application.Resources;

namespace SnapNFix.Application.Features.Issue.Queries.GetSnapReportsByIssueId;
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
        var issueExists = _unitOfWork.Repository<Domain.Entities.Issue>()
            .ExistsById(request.Id);

        if (!issueExists)
        {
            return GenericResponseModel<PagedList<ReportDetailsDto>>.Failure(
                Shared.IssueNotFound,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create(nameof(request.Id), Shared.IssueNotFound)
                });
        }
        var query = _unitOfWork.Repository<Domain.Entities.SnapReport>()
            .GetQuerableData()
            .Where(r => r.IssueId == request.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Include(r => r.User);

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