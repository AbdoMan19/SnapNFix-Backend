using MediatR;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Issue.DTOs;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.Issue.Queries;

public class GetIssuesQuery : IRequest<GenericResponseModel<PagedList<IssueDetailsDto>>>
{
    public IssueStatus? Status { get; set; }
    public ReportCategory? Category { get; set; }
    public Severity? Severity { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}