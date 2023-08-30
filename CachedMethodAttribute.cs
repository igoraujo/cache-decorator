
namespace cache_decorator;

public class CachedAttribute : CacheAttribute
{
    public string Key { get; set; }

    public CachedAttribute()
    {

    }

    // public CachedAttribute(string key)
    //     : base(key)
    // {
    // }
}
