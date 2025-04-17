using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SnapNFix.Application.Common.ResponseModel;
using SnapNFix.Application.Features.Auth.Dtos;
using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces;

namespace SnapNFix.Infrastructure.Services.UserService;

public class UserService(UserManager<User> userManager , CancellationToken cancellationToken) : IUserService
{
    public async  Task<(bool isEmail, bool isPhone, User? user)> IsEmailOrPhone(string emailOrPhone)
    {
        User user = null;
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
            user = await userManager.Users
                .FirstOrDefaultAsync(u => u.PhoneNumber ==emailOrPhone, cancellationToken);
        }

        return (isEmail, isPhone, user);
    }
}