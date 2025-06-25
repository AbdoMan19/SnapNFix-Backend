using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.FastReport.DTOs;
using SnapNFix.Application.Resources;
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

        var issueExists = _unitOfWork.Repository<Domain.Entities.Issue>()
            .ExistsById(request.Id);

        if (!issueExists)
        {
            return GenericResponseModel<PagedList<FastReportDetailsDto>>.Failure(
                Shared.IssueNotFound,
                new List<ErrorResponseModel>
                {
                    ErrorResponseModel.Create(nameof(request.Id), Shared.IssueNotFound)
                });
        }

        var query = _unitOfWork.Repository<Domain.Entities.FastReport>()
            .GetQuerableData()
            .Where(r => r.IssueId == request.Id)
            .OrderByDescending(r => r.CreatedAt)
            .Include(r => r.User);

        var result = await PagedList<FastReportDetailsDto>.CreateAsync(
            query.Select(q => new FastReportDetailsDto
            {
                Id = q.Id,
                IssueId = q.IssueId,
                UserId = q.UserId,
                CreatedAt = q.CreatedAt,
                Comment = q.Comment,
                FirstName = q.User.FirstName,
                LastName = q.User.LastName,
                Severity = q.Severity,
                
            }),
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        
        return GenericResponseModel<PagedList<FastReportDetailsDto>>.Success(result);
    }
}