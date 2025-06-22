using SnapNFix.Application.Common.Interfaces.ServiceLifetime;

public interface ICacheService : IScoped
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class;
    Task RemoveAsync(string key);
    Task RemovePatternAsync(string pattern);
    Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> getItem, TimeSpan? expiration = null) where T : class;
}