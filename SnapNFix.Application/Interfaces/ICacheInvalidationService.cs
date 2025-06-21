
namespace SnapNFix.Application.Common.Interfaces;

public interface ICacheInvalidationService
{

    Task InvalidateCacheAsync(string key, CancellationToken cancellationToken = default);
    
    Task InvalidateCacheByPatternAsync(string pattern, CancellationToken cancellationToken = default);
}