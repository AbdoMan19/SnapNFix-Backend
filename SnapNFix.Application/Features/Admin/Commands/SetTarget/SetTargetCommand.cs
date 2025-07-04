using MediatR;
using SnapNFix.Application.Common.ResponseModel;

public class SetTargetCommand : IRequest<GenericResponseModel<bool>>
{
    public double TargetResolutionRate { get; set; }
}