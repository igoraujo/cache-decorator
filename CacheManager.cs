namespace cache_decorator;

public class CacheManager
{
    private readonly Dictionary<string, object> _cache = new();

    public bool TryGetValue(string key, out object value)
    {
        return _cache.TryGetValue(key, out value);
    }

    public void Add(string key, object value)
    {
        _cache[key] = value;
    }
}

