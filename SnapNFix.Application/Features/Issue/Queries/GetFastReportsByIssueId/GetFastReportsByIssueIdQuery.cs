using MediatR;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.FastReport.DTOs;

namespace SnapNFix.Application.Features.Issue.Queries;

public class GetFastReportsByIssueIdQuery : IRequest<GenericResponseModel<PagedList<FastReportDetailsDto>>>
{
    public Guid Id { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}