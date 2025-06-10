using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Issue.DTOs;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Issue.Queries;

public class GetIssueByIdQueryHandler : 
    IRequestHandler<GetIssueByIdQuery, GenericResponseModel<IssueDetailsDto>>
{
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;

    public GetIssueByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
    {
        _mapper = mapper;
        _unitOfWork = unitOfWork;
    }

    public async Task<GenericResponseModel<IssueDetailsDto>> Handle(
        GetIssueByIdQuery request, CancellationToken cancellationToken)
    {
        var issue = await _unitOfWork.Repository<Domain.Entities.Issue>()
            .FindBy(i => i.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (issue == null)
        {
            return GenericResponseModel<IssueDetailsDto>.Failure("Issue not found");
        }

        var issueDto = _mapper.Map<IssueDetailsDto>(issue);
        
        return GenericResponseModel<IssueDetailsDto>.Success(issueDto);
    }
}