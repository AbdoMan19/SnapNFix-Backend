using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.SnapReport.DTOs;

namespace SnapNFix.Application.Features.SnapReport.Queries;

public class GetAllReportQuery : IRequest<GenericResponseModel<List<ReportDetailsDto>>>
{
    
}