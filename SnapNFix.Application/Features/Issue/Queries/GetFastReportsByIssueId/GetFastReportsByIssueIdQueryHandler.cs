using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.FastReport.DTOs;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Issue.Queries;

public class GetFastReportsByIssueIdQueryHandler : 
    IRequestHandler<GetFastReportsByIssueIdQuery, GenericResponseModel<PagedList<FastReportDetailsDto>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetFastReportsByIssueIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<GenericResponseModel<PagedList<FastReportDetailsDto>>> Handle(
        GetFastReportsByIssueIdQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Domain.Entities.FastReport>()
            .GetQuerableData()
            .Where(r => r.IssueId == request.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Include(r => r.User);

        var reports = await PagedList<Domain.Entities.FastReport>.CreateAsync(
            query,
            request.PageNumber,
            10,
            cancellationToken);

        var mappedItems = reports.Items.Select(fr => new FastReportDetailsDto
        {
            Id = fr.Id,
            UserId = fr.UserId,
            IssueId = fr.IssueId,
            Comment = fr.Comment,
            CreatedAt = fr.CreatedAt,
            UserFirstName = fr.User.FirstName,
            UserLastName = fr.User.LastName
        }).ToList();

        var reportDtos = new PagedList<FastReportDetailsDto>(
            mappedItems,
            reports.TotalCount,
            reports.PageNumber,
            reports.PageSize);

        return GenericResponseModel<PagedList<FastReportDetailsDto>>.Success(reportDtos);
    }
}