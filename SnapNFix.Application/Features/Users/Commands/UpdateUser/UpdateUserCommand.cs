using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Enums;

namespace SnapNFix.Application.Features.Users.Commands.UpdateUser;

public record UpdateUserCommand : IRequest<GenericResponseModel<bool>>
{
    public Gender? Gender { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}