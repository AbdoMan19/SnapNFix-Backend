using MapsterMapper;
using MediatR;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Issue.DTOs;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Issue.Queries;

public class GetIssuesQueryHandler : 
    IRequestHandler<GetIssuesQuery, GenericResponseModel<PagedList<IssueDetailsDto>>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetIssuesQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<GenericResponseModel<PagedList<IssueDetailsDto>>> Handle(
        GetIssuesQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Domain.Entities.Issue>()
            .GetQuerableData();
        
        if (request.Status.HasValue)
        {
            query = query.Where(i => i.Status == request.Status.Value);
        }

        if (request.Category.HasValue)
        {
            query = query.Where(i => i.Category == request.Category.Value);
        }

        if (request.Severity.HasValue)
        {
            query = query.Where(i => i.Severity == request.Severity.Value);
        }

        query = query.OrderByDescending(i => i.CreatedAt);

        var issues = await PagedList<Domain.Entities.Issue>.CreateAsync(
            query, 
            request.PageNumber, 
            request.PageSize,
            cancellationToken);

        var mappedItems = _mapper.Map<List<IssueDetailsDto>>(issues.Items);
        
        var issueDtos = new PagedList<IssueDetailsDto>(
            mappedItems,
            issues.TotalCount,
            issues.PageNumber,
            issues.PageSize);

        return GenericResponseModel<PagedList<IssueDetailsDto>>.Success(issueDtos);
    }
}