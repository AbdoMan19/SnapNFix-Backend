using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Application.Interfaces;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Infrastructure.Services.UserService;

public class UserService(UserManager<User> userManager , IHttpContextAccessor contextAccessor , IUnitOfWork unitOfWork) : IUserService
{
    public async  Task<(bool isEmail, bool isPhone, User? user)> GetUserByEmailOrPhoneNumber(string emailOrPhone)
    {
        User user;
        var isEmail = false;
        var isPhone = false;

        if (emailOrPhone.Contains("@"))
        {
            isEmail = true;
            user = await userManager.FindByEmailAsync(emailOrPhone);
        }
        else
        {
            isPhone = true;
            user = await userManager.Users.AsNoTracking().FirstOrDefaultAsync(u => u.PhoneNumber ==emailOrPhone);
        }

        return (isEmail, isPhone, user);
    }
    public async Task<User?> GetCurrentUserAsync()
    {
        var userId = userManager.GetUserId( contextAccessor.HttpContext?.User);
        var user = await userManager.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id.ToString() == userId);
        return user;
    }
    
    public  Task<Guid> GetCurrentUserIdAsync()
    {
        var userId =  userManager.GetUserId( contextAccessor.HttpContext?.User);
        return userId != null ? Task.FromResult(Guid.Parse(userId)) : Task.FromResult(Guid.Empty);
    }
   
    
    

}