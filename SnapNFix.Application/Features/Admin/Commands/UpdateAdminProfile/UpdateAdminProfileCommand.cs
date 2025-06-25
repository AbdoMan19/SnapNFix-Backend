using MediatR;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Enums;

public class UpdateAdminProfileCommand : IRequest<GenericResponseModel<bool>>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public DateOnly? BirthDate { get; set; }
    public Gender? Gender { get; set; }
}