using AgileSouthwestCMSAPI.Api.Requests.inventory;
using AgileSouthwestCMSAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[Authorize(Policy = "TenantAdmin")]
public class StoresController(IStoresService service): ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddStoreRequest request)
    {
        var result = await service.AddStore(request);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var store = await service.GetStore(id);
        return Ok(store);
    }
}