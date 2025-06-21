using System;
using System.Threading;
using System.Threading.Tasks;

namespace SnapNFix.Application.Common.Interfaces;


public interface ICacheService
{

    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    

    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default);
    

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    

    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
}