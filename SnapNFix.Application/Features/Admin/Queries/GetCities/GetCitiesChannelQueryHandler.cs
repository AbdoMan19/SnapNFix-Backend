using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Admin.DTOs;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Admin.Queries.GetCities
{
    public class GetCitiesChannelQueryHandler : 
        IRequestHandler<GetCitiesChannelQuery, GenericResponseModel<PagedList<CityChannelDto>>>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;

        public GetCitiesChannelQueryHandler(IMapper mapper, IUnitOfWork unitOfWork)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
        }

        public async Task<GenericResponseModel<PagedList<CityChannelDto>>> Handle(
            GetCitiesChannelQuery request, CancellationToken cancellationToken)
        {
            // Get query from repository
            var query = _unitOfWork.Repository<Domain.Entities.CityChannel>()
                .GetQuerableData();

            // Apply filters
            if (request.IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == request.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(c => 
                    c.Name.ToLower().Contains(searchTerm) || 
                    c.State.ToLower().Contains(searchTerm));
            }

            // Include subscription count
            var citiesWithCounts = query
                .Select(c => new CityChannelDto
                {
                    Name = c.Name,
                    State = c.State,
                    Country = c.Country,
                    IsActive = c.IsActive,
                    SubscriberCount = c.Subscriptions.Count,
                    IssueCount = _unitOfWork.Repository<Domain.Entities.Issue>()
                        .GetQuerableData()
                        .Count(i => i.City == c.Name && i.State == c.State)
                });

            // Create paged list
            var result = await PagedList<CityChannelDto>.CreateAsync(
                citiesWithCounts,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            return GenericResponseModel<PagedList<CityChannelDto>>.Success(result);
        }
    }
}