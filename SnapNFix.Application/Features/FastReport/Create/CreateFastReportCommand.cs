using FluentValidation;
using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using Severity = SnapNFix.Domain.Enums.Severity;

namespace SnapNFix.Application.Features.FastReport.Create;

public class CreateFastReportCommand : IRequest<GenericResponseModel<bool>>
{
    public Guid IssueId { get; set; }
    public string Comment { get; set; }
    public Severity Severity { get; set; } = Severity.Low;
}