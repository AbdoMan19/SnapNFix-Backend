using MediatR;
using SnapNFix.Application.Common.ResponseModel;

namespace SnapNFix.Application.Features.FastReport.Create;

public class CreateFastReportCommand : IRequest<GenericResponseModel<bool>>
{
    public Guid IssueId { get; set; }
    public string Comment { get; set; }
}