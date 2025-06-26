using MediatR;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Users.DTOs;
using SnapNFix.Application.Interfaces;
using SnapNFix.Application.Resources;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Application.Features.Users.Queries.GetAvailableCitiesChannelQueries;

public class GetAvailableCitiesChannelQueryHandler: 
    IRequestHandler<GetAvailableCitiesChannelQuery, GenericResponseModel<PagedList<CityChannelSubscriptionDto>>>
{
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;

        public GetAvailableCitiesChannelQueryHandler(IUnitOfWork unitOfWork, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _userService = userService;
        }

        public async Task<GenericResponseModel<PagedList<CityChannelSubscriptionDto>>> Handle(
            GetAvailableCitiesChannelQuery request, CancellationToken cancellationToken)
        {
            var currentUserId = await _userService.GetCurrentUserIdAsync();
            if (currentUserId == Guid.Empty)
            {
                return GenericResponseModel<PagedList<CityChannelSubscriptionDto>>.Failure(Shared.UserNotAuthenticated,
                    new List<ErrorResponseModel>
                    {
                        ErrorResponseModel.Create(nameof(currentUserId), Shared.UserNotAuthenticated)
                    });
            }

            // Get all active cities
            var query = _unitOfWork.Repository<Domain.Entities.CityChannel>()
                .GetQuerableData()
                .Where(c => c.IsActive);

            // Apply search filter if provided
            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(searchTerm) ||
                    c.State.ToLower().Contains(searchTerm));
            }

            // Get user's existing subscriptions for checking
            var userSubscriptions = await _unitOfWork.Repository<UserCitySubscription>()
                .GetQuerableData()
                .Where(s => s.UserId == currentUserId)
                .Select(s => s.CityChannelId)
                .ToListAsync(cancellationToken);

            // Apply paging
            var pagedCities = await PagedList<Domain.Entities.CityChannel>.CreateAsync(
                query,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            // Transform to DTOs with subscription status and active issue counts
            var result = new List<CityChannelSubscriptionDto>();

            foreach (var city in pagedCities.Items.Where(c => !userSubscriptions.Contains(c.Id)))
            {
                // Count active issues in this city
                var activeIssueCount = await _unitOfWork.Repository<Domain.Entities.Issue>()
                    .GetQuerableData()
                    .CountAsync(i =>
                            i.City == city.Name &&
                            i.State == city.State &&
                            i.Status != IssueStatus.Completed,
                        cancellationToken);

                result.Add(new CityChannelSubscriptionDto
                {
                    Id = city.Id,
                    Name = city.Name,
                    State = city.State,
                    ActiveIssuesCount = activeIssueCount,
                });
            }

            var pagedResult = new PagedList<CityChannelSubscriptionDto>(
                result,
                pagedCities.TotalCount,
                pagedCities.PageNumber,
                pagedCities.PageSize);

            return GenericResponseModel<PagedList<CityChannelSubscriptionDto>>.Success(pagedResult);
        }

}