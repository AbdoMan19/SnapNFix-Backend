using MediatR;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.SnapReport.DTOs;

namespace SnapNFix.Application.Features.SnapReport.Queries;

public class GetUserReportsQuery : IRequest<GenericResponseModel<PagedList<ReportDetailsDto>>>
{
    public string? Status { get; set; }
    public string? Category { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}