using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Statistics.Queries.GetCategoryDistribution;

public class GetCategoryDistributionQuery : IRequest<GenericResponseModel<List<CategoryDistributionDto>>>
{
}