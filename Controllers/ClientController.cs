using Microsoft.AspNetCore.Mvc;

namespace cache_decorator.Controllers;

[ApiController]
[Route("[controller]")]
public class ClienteController(MyClass myClass) : ControllerBase
{
    [HttpGet("buscarcpf/{id}")]
    public async Task<IActionResult> BuscarCpf(int id)
    {
        var cpf = await myClass.BuscarCpfCachedAsync(id);
        return Ok(cpf);
    }
}
