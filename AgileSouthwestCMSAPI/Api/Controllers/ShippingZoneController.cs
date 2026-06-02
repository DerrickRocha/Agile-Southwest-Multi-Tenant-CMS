using AgileSouthwestCMSAPI.Api.Requests.ShippingZones;
using AgileSouthwestCMSAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ShippingZoneController(IShippingZoneService service): ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> Post([FromBody]ShippingZoneRequest request)
    {
        var result = await service.AddShippingZone(request);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        return Ok(await service.GetShippingZone(id));
    }
}