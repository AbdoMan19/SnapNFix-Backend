using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.SnapReport.DTOs;
using MediatR;
using SnapNFix.Application.Common.Models;

namespace SnapNFix.Application.Features.Issue.Queries;

public class GetSnapReportsByIssueIdQuery : IRequest<GenericResponseModel<PagedList<ReportDetailsDto>>>
{
    public Guid Id { get; set; }
    public int PageNumber { get; set; } = 1;

}
