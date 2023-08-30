namespace cache_decorator;

public class CacheAspect
{
    private readonly CacheManager _cacheManager;

    public CacheAspect(CacheManager cacheManager)
    {
        _cacheManager = cacheManager;
    }

    public T ExecuteWithCache<T>(Func<T> method, string key)
    {
        if (_cacheManager.TryGetValue(key, out var cachedValue))
        {
            return (T)cachedValue;
        }

        var result = method();

        if (result != null)
        {
            _cacheManager.Add(key, result);
        }

        return result;
    }
}
