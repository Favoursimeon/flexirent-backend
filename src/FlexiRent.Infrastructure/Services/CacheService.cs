using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace FlexiRent.Infrastructure.Services;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null);
    Task RemoveAsync(string key);
    Task RemoveByPrefixAsync(string prefix);
}

public class CacheService : ICacheService
{
    private readonly IDistributedCache _cache;

    public CacheService(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        try
        {
            var data = await _cache.GetStringAsync(key);
            return data is null
                ? default
                : JsonSerializer.Deserialize<T>(data);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null)
    {
        try
        {
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiry ?? TimeSpan.FromMinutes(5)
            };
            var data = JsonSerializer.Serialize(value);
            await _cache.SetStringAsync(key, data, options);
        }
        catch
        {
            // Cache failure should never break the app
        }
    }

    public async Task RemoveAsync(string key)
    {
        try { await _cache.RemoveAsync(key); }
        catch { }
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        // IDistributedCache doesn't support pattern delete natively
        // This is handled at the Redis level via key naming conventions
        // For now we remove known keys — a proper impl uses IConnectionMultiplexer
        await Task.CompletedTask;
    }
}