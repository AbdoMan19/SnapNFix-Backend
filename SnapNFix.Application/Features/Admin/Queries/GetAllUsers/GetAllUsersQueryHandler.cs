using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Common.Models;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Enums;
using SnapNFix.Domain.Interfaces;

public class GetAllUsersQueryHandler : IRequestHandler<GetAllUsersQuery, GenericResponseModel<PagedList<UserDetailsDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly UserManager<User> _userManager;
    private readonly ICacheService _cacheService;

    public GetAllUsersQueryHandler(
        IUnitOfWork unitOfWork,
        UserManager<User> userManager,
        ICacheService cacheService)
    {
        _unitOfWork = unitOfWork;
        _userManager = userManager;
        _cacheService = cacheService;
    }

    public async Task<GenericResponseModel<PagedList<UserDetailsDto>>> Handle(
        GetAllUsersQuery request, 
        CancellationToken cancellationToken)
    {

   

        var query = _unitOfWork.Repository<User>()
            .GetQuerableData()
            .AsQueryable();

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var searchTerm = request.SearchTerm.ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(searchTerm) ||
                u.LastName.ToLower().Contains(searchTerm) ||
                (u.Email != null && u.Email.ToLower().Contains(searchTerm)) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(searchTerm)));
        }

        // Apply role filter if specified
        if (request.Role.HasValue)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(request.Role.ToString());
            var userIdsInRole = usersInRole.Select(u => u.Id).ToHashSet();
            query = query.Where(u => userIdsInRole.Contains(u.Id));
        }

        var pagedUsers = await PagedList<User>.CreateAsync(
            query,
            request.PageNumber,
            request.PageSize,
            cancellationToken);

        var userDtos = new List<UserDetailsDto>();

        foreach (var user in pagedUsers.Items)
        {
            var roles = await _userManager.GetRolesAsync(user);
            
            var userDto = new UserDetailsDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                IsSuspended = user.IsSuspended,
                IsDeleted = user.IsDeleted,
                CreatedAt = user.CreatedAt,
                Roles = roles.ToList() 
            };

            userDtos.Add(userDto);
        }

        var result = new PagedList<UserDetailsDto>(
            userDtos,
            pagedUsers.TotalCount,
            pagedUsers.PageNumber,
            pagedUsers.PageSize);


        return GenericResponseModel<PagedList<UserDetailsDto>>.Success(result);
    }
}