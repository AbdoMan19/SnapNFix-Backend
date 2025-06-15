using FluentValidation;
using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.Statistics.Queries.GetGeographicDistribution;

public class GetGeographicDistributionQuery : IRequest<GenericResponseModel<List<GeographicDistributionDto>>>
{
    public int Limit { get; set; } = 10;
}