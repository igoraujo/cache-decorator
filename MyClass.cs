namespace cache_decorator;

public class MyClass
{
    private readonly CacheAspect _cacheAspect;

    public MyClass(CacheAspect cacheAspect)
    {
        _cacheAspect = cacheAspect;
    }

    // [Cached]
    private string BuscarCpf(int id)
    {
        return id switch 
        {
            0 => "00000000000",
            1 => "11111111111",
            2 => "22222222222",
            _ => null,
        };

    }

    public string BuscarCpfCached(int id)
    {
        return _cacheAspect.ExecuteWithCache(() => BuscarCpf(id), $"cpfCache:{id}");
    }
}
