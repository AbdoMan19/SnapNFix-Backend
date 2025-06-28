using MediatR;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

public class GetCurrentTargetQueryHandler : IRequestHandler<GetCurrentTargetQuery, GenericResponseModel<double>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCurrentTargetQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GenericResponseModel<double>> Handle(GetCurrentTargetQuery request, CancellationToken cancellationToken)
    {
        var currentTarget = await _unitOfWork.Repository<AdminTarget>()
            .FindBy(t => t.IsActive)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        var targetValue = currentTarget?.TargetResolutionRate ?? 95.0; 
        return GenericResponseModel<double>.Success(targetValue);
    }
}