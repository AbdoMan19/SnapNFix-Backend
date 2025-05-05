using System.Net;
using SnapNFix.Domain.Interfaces.ServiceLifetime;

namespace SnapNFix.Api.RateLimiting.Interfaces;

public interface IRateLimiter : ISingleton
{
    Task<bool> IsIpBlockedAsync(IPAddress ipAddress, string path, RateLimitRule rule , CancellationToken cancellationToken);
}