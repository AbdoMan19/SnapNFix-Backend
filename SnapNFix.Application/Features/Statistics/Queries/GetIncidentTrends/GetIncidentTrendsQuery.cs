using FluentValidation;
using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Statistics.Queries.GetIncidentTrends;

public class GetIncidentTrendsQuery : IRequest<GenericResponseModel<List<IncidentTrendDto>>>
{
    public StatisticsInterval Interval { get; set; } = StatisticsInterval.Monthly;
}