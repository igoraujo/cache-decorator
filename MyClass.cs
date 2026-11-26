using cache_decorator.Cache;

namespace cache_decorator;

public class MyClass(Cache.CacheAspect cacheAspect)
{
    // [Cached]
    private static string? BuscarCpf(int id)
    {
        return id switch 
        {
            0 => "00000000000",
            1 => "11111111111",
            2 => "22222222222",
            _ => null,
        };

    }

    public async Task<string?> BuscarCpfCachedAsync(int id)
    {
        return await cacheAspect.ExecuteWithCacheAsync(
            () => Task.FromResult(BuscarCpf(id)),
            $"cache:identificacao:cpf:{id}",
            CacheDuration.TwentyFiveMinutes);
    }
}
