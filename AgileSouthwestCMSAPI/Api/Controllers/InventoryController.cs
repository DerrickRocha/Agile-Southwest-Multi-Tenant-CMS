using AgileSouthwestCMSAPI.Api.Requests.inventory;
using AgileSouthwestCMSAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[Authorize(Policy = "TenantAdmin")]
public class InventoryController(IInventoryService service) : ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> AddItem([FromBody] AddItemToInventoryRequest request)
    {
        var result = await service.AddToInventory(request);
        return CreatedAtAction(nameof(GetItem), new { id = result.Id }, result);
    }

    [HttpGet]
    public async Task<IActionResult> GetItem(int id)
    {
        var result = await service.GetInventoryItem(id);
        return Ok(result);
    }
}