using SnapNFix.Domain.Entities;

namespace SnapNFix.Domain.Interfaces;

public interface IUserService
{
    public Task<(bool isEmail, bool isPhone, User? user)> GetUserByEmailOrPhoneNumber(string emailOrPhone);
    public Task<User?> GetCurrentUserAsync();
    public Task<Guid> GetCurrentUserIdAsync();
}