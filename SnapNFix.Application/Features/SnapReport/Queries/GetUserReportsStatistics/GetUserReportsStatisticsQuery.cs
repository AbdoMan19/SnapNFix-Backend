using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.SnapReport.DTOs;

namespace SnapNFix.Application.Features.SnapReport.Queries.GetUserReportsStatistics;

public record GetUserReportsStatisticsQuery : IRequest<GenericResponseModel<UserReportsStatisticsDto>>
{
    
}