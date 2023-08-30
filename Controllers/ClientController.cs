using Microsoft.AspNetCore.Mvc;

namespace cache_decorator.Controllers;

[ApiController]
[Route("[controller]")]
public class ClienteController : ControllerBase
{
    private readonly MyClass _myClass;

    public ClienteController(MyClass myClass)
    {
        _myClass = myClass;
    }

    [HttpGet("buscarcpf/{id}")]
    public IActionResult BuscarCpf(int id)
    {
        var cpf = _myClass.BuscarCpfCached(id);
        return Ok(cpf);
    }
}
