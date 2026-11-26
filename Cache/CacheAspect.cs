namespace cache_decorator.Cache;

public class CacheAspect(CacheManager cacheManager)
{
    public async Task<T> ExecuteWithCacheAsync<T>(Func<Task<T>> method, string key, TimeSpan expirationDuration)
    {
        var cachedValue = await cacheManager.GetAsync<T>(key);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        var result = await method();

        if (result != null)
        {
            await cacheManager.SetAsync(key, result, expirationDuration);
        }

        return result;
    }

    public async Task<T> ExecuteWithCacheAsync<T>(Func<Task<T>> method, string key, Func<T, TimeSpan> expirationDurationFactory)
    {
        var cachedValue = await cacheManager.GetAsync<T>(key);
        if (cachedValue != null)
        {
            return cachedValue;
        }

        var result = await method();

        if (result != null)
        {
            var expirationDuration = expirationDurationFactory?.Invoke(result) ?? CacheDuration.FiveMinutes;
            await cacheManager.SetAsync(key, result, expirationDuration);
        }

        return result;
    }

    public async Task RemoveCacheAsync(string key) => await cacheManager.RemoveCacheAsync(key);

    public async Task RemoveAllCacheAsync() => await cacheManager.RemoveAllCacheAsync();

}