using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Statistics.Queries.GetDashboardSummary;

public class GetDashboardSummaryQuery : IRequest<GenericResponseModel<StatisticsDto>>
{
}
