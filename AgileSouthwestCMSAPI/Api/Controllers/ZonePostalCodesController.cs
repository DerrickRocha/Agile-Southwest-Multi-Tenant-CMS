using AgileSouthwestCMSAPI.Api.Requests.ZonePostalCodes;
using AgileSouthwestCMSAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ZonePostalCodesController(IZonePostalCodeService service): ControllerBase
{
    
    public async Task<IActionResult> Post([FromBody] AddZonePostalCodeRequest request)
    {
        var result = await service.AddZonePostalCode(request);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await service.GetZonePostalCode(id);
        return Ok(result);
    }
    
}