using SnapNFix.Domain.Entities;

namespace SnapNFix.Domain.Interfaces;

public interface IUserService
{
    public Task<(bool isEmail, bool isPhone, User user)> IsEmailOrPhone(string emailOrPhone);
}