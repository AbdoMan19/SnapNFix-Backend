using Mapster;
using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.SnapReport.DTOs;
using SnapNFix.Application.Interfaces;

namespace SnapNFix.Application.Features.SnapReport.Queries;

public class GetAllReportQueryHandler : IRequestHandler<GetAllReportQuery , GenericResponseModel<List<ReportDetailsDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    public GetAllReportQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
    
    public async Task<GenericResponseModel<List<ReportDetailsDto>>> Handle(GetAllReportQuery request, CancellationToken cancellationToken)
    {
        var reports = (await _unitOfWork.Repository<Domain.Entities.SnapReport>().GetAll()).ToList();
        return GenericResponseModel<List<ReportDetailsDto>>.Success(reports.Adapt<List<ReportDetailsDto>>());

    }
}