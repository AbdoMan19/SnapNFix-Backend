using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Issue.DTOs;

namespace SnapNFix.Application.Features.Issue.Queries;

public class GetIssueByIdQuery : IRequest<GenericResponseModel<IssueDetailsDto>>
{
    public Guid Id { get; set; }
}