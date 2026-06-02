using AgileSouthwestCMSAPI.Api.Requests.ShippingRates;
using AgileSouthwestCMSAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ShippingRateController(IShippingRateService service): ControllerBase
{
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetShippingRate(int id)
    {
        var result = await service.GetShippingRate(id);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> AddShippingRate([FromBody] AddShippingRateRequest request)
    {
        var result = await service.AddShippingRate(request);
        return CreatedAtAction(nameof(GetShippingRate), new { id = result.Id }, result);
    }
}