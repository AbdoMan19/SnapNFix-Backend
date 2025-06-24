using MediatR;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Enums;

public class GetAllUsersQuery : IRequest<GenericResponseModel<PagedList<UserDetailsDto>>>
{
    public string? SearchTerm { get; set; }
    public UserRole? Role { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}