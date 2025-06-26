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
        IRequestHandler<GetUserSubscribedCitiesChannelCommand, GenericResponseModel<PagedList<CityChannelSubscriptionDto>>>
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

        public async Task<GenericResponseModel<PagedList<CityChannelSubscriptionDto>>> Handle(
            GetUserSubscribedCitiesChannelCommand request, CancellationToken cancellationToken)
        {
            var currentUserId = await _userService.GetCurrentUserIdAsync();
            if (currentUserId == Guid.Empty)
            {
                return GenericResponseModel<PagedList<CityChannelSubscriptionDto>>.Failure("User not authenticated");
            }

            var subscriptions = _unitOfWork.Repository<UserCitySubscription>()
                .GetQuerableData()
                .Where(s => s.UserId == currentUserId && s.CityChannel.IsActive)
                .Include(s => s.CityChannel)
                .OrderByDescending(s => s.SubscribedAt)
                .Select(s => new CityChannelSubscriptionDto
                {
                    Id = s.CityChannelId,
                    Name = s.CityChannel.Name,
                    State = s.CityChannel.State,
                    ActiveIssuesCount = _unitOfWork.Repository<Domain.Entities.Issue>().GetQuerableData()
                        .Count(i => i.City == s.CityChannel.Name &&
                                    i.State == s.CityChannel.State &&
                                    i.Status != IssueStatus.Completed)
                });
            var pagedSubscriptions = await PagedList<CityChannelSubscriptionDto>.CreateAsync(
                subscriptions,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            return GenericResponseModel<PagedList<CityChannelSubscriptionDto>>.Success(pagedSubscriptions);
        }
    }
