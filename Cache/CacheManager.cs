using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;

namespace cache_decorator.Cache;

public class CacheManager(IDistributedCache cache)
{
    private readonly List<string> cacheKeys = [];

    public async Task<T> GetAsync<T>(string key)
    {
        var cachedData = await cache.GetStringAsync(key);
        return cachedData is null ? default : JsonSerializer.Deserialize<T>(cachedData);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expirationDuration)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expirationDuration
        };

        if (!cacheKeys.Contains(key))
        {
            cacheKeys.Add(key);
        }

        var jsonData = JsonSerializer.Serialize(value);
        await cache.SetStringAsync(key, jsonData, options);
    }

    public async Task RemoveCacheAsync(string key) => await cache.RemoveAsync(key);

    public async Task RemoveAllCacheAsync()
    {
        cacheKeys.ForEach(async key => await cache.RemoveAsync(key));
        cacheKeys.Clear();
    }
}

