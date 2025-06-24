using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Users.DTOs;
using SnapNFix.Application.Features.Users.Queries.GetUserSubscribedCitiesChannel;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;


namespace SnapNFix.Application.Features.Users.Queries.GetUserSubscribedCitiesChannel;

    public class GetUserSubscribedCitiesChannelCommandHandler : 
        IRequestHandler<GetUserSubscribedCitiesChannelCommand, GenericResponseModel<PagedList<UserCitySubscriptionDto>>>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserService _userService;

        public GetUserSubscribedCitiesChannelCommandHandler(
            IMapper mapper, 
            IUnitOfWork unitOfWork,
            IUserService userService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _userService = userService;
        }

        public async Task<GenericResponseModel<PagedList<UserCitySubscriptionDto>>> Handle(
            GetUserSubscribedCitiesChannelCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = await _userService.GetCurrentUserIdAsync();
            if (currentUserId == Guid.Empty)
            {
                return GenericResponseModel<PagedList<UserCitySubscriptionDto>>.Failure("User not authenticated");
            }

            var subscriptions = _unitOfWork.Repository<UserCitySubscription>()
                .GetQuerableData()
                .Where(s => s.UserId == currentUserId)
                .Include(s => s.CityChannel)
                .OrderByDescending(s => s.SubscribedAt)
                .Select(s => new UserCitySubscriptionDto
                {
                    CityId = s.CityChannelId,
                    CityName = s.CityChannel.Name,
                    State = s.CityChannel.State,
                    ActiveIssuesCount = _unitOfWork.Repository<Domain.Entities.Issue>().GetQuerableData()
                        .Count(i => i.City == s.CityChannel.Name &&
                                    i.State == s.CityChannel.State &&
                                    i.Status != IssueStatus.Completed)
                });
            var pagedSubscriptions = await PagedList<UserCitySubscriptionDto>.CreateAsync(
                subscriptions,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            return GenericResponseModel<PagedList<UserCitySubscriptionDto>>.Success(pagedSubscriptions);
        }
    }
