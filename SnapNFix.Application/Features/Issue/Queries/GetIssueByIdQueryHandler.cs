using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Issue.DTOs;
using SnapNFix.Domain.Interfaces;
using SnapNFix.Domain.Enums;

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
            .Include(i => i.AssociatedSnapReports)
            .FirstOrDefaultAsync(cancellationToken);

        if (issue == null)
        {
            return GenericResponseModel<IssueDetailsDto>.Failure("Issue not found");
        }

        var associatedImages = issue.AssociatedSnapReports
            .Where(sr => sr.ImageStatus == ImageStatus.Approved && !string.IsNullOrEmpty(sr.ImagePath))
            .OrderBy(sr => sr.CreatedAt)
            .Take(5)
            .Select(sr => sr.ImagePath)
            .ToList();

        var issueDto = new IssueDetailsDto
        {
            Id = issue.Id,
            ImagePath = issue.ImagePath,
            Category = issue.Category.ToString(),
            Latitude = issue.Location.Y,
            Longitude = issue.Location.X,
            CreatedAt = issue.CreatedAt,
            Status = issue.Status.ToString(),
            Severity = issue.Severity.ToString(),
            AssociatedImages = associatedImages
        };
        
        return GenericResponseModel<IssueDetailsDto>.Success(issueDto);
    }
}