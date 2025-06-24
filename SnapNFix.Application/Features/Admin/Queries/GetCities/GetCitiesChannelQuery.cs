using MediatR;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Admin.DTOs;

namespace SnapNFix.Application.Features.Admin.Queries.GetCities;

public class GetCitiesChannelQuery : IRequest<GenericResponseModel<PagedList<CityChannelDto>>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool? IsActive { get; set; }
    public string? SearchTerm { get; set; }
}