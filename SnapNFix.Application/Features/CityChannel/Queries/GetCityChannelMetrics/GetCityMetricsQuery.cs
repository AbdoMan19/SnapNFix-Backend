using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.CityChannel.DTOs;

namespace SnapNFix.Application.Features.CityChannel.Queries.GetCityChannelMetrics;

public class GetCityChannelMetricsQuery : IRequest<GenericResponseModel<CityMetricsDto>>
{
    public Guid CityId { get; set; }
}