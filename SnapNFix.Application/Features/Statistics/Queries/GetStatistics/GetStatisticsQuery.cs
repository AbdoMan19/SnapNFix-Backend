using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Statistics.Queries.GetStatistics;

public class GetStatisticsQuery : IRequest<GenericResponseModel<StatisticsDto>>
{
}