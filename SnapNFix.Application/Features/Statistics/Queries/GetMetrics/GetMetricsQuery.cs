using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Statistics.Queries.GetMetrics;

public class GetMetricsQuery : IRequest<GenericResponseModel<MetricsOverviewDto>>
{
}