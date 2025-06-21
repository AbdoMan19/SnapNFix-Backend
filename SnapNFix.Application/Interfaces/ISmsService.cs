using SnapNFix.Application.Common.Interfaces.ServiceLifetime;

namespace SnapNFix.Application.Common.Interfaces;

public interface ISmsService : IScoped
{
    Task<bool> SendSmsAsync(string phoneNumber, string message);
}


