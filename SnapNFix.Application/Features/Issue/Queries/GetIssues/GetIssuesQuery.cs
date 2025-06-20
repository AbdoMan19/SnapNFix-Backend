using MediatR;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Issue.DTOs;

namespace SnapNFix.Application.Features.Issue.Queries;

public class GetIssuesQuery : IRequest<GenericResponseModel<PagedList<IssueDetailsDto>>>
{
    public string? Status { get; set; }
    public string? Category { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}