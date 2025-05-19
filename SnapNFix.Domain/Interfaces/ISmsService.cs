using SnapNFix.Domain.Interfaces.ServiceLifetime;

namespace SnapNFix.Domain.Interfaces;

public interface ISmsService : IScoped
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
}