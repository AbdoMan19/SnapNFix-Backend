using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.Statistics.Queries.GetMonthlyTarget;

public class GetMonthlyTargetQuery : IRequest<GenericResponseModel<MonthlyTargetDto>>
{
}