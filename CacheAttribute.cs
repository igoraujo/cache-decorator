namespace cache_decorator;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public class CacheAttribute : Attribute
{
    public CacheAttribute()
    {
    }
}