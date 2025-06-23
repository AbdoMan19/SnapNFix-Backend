using SnapNFix.Application.Common.Interfaces.ServiceLifetime;

namespace SnapNFix.Application.Interfaces;

public interface ISmsService : IScoped
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
}


