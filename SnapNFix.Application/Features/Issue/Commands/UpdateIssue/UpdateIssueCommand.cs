using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.Issue.Commands.UpdateIssue;

public class UpdateIssueCommand : IRequest<GenericResponseModel<bool>>
{
    public Guid Id { get; set; }
    public IssueStatus? Status { get; set; } 
    public Severity? Severity { get; set; }
}