using SnapNFix.Domain.Entities;
using SnapNFix.Domain.Interfaces.ServiceLifetime;

namespace SnapNFix.Domain.Interfaces;

public interface IUserService : IScoped
{
    public Task<(bool isEmail, bool isPhone, User? user)> GetUserByEmailOrPhoneNumber(string emailOrPhone);
    public Task<User?> GetCurrentUserAsync();
    public Task<Guid> GetCurrentUserIdAsync();
}