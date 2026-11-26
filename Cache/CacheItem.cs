namespace cache_decorator.Cache;

public class CacheItem(object value, TimeSpan expirationDuration)
{
    public object Value { get; } = value;
    public DateTime Expiration { get; } = DateTime.UtcNow.Add(expirationDuration);
    public bool IsExpired => DateTime.UtcNow > Expiration;
}

