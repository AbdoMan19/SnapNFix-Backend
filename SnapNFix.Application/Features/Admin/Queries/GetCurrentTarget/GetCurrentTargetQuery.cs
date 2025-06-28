using MediatR;
using SnapNFix.Application.Common.ResponseModel;

public class GetCurrentTargetQuery : IRequest<GenericResponseModel<double>>
{
}