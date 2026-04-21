using AgileSouthwestCMSAPI.Api.Requests.Images;
using AgileSouthwestCMSAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ImagesController(IImagesService service): ControllerBase
{
    public async Task<IActionResult> Post([FromBody] AddImagesRequest request)
    {
        var result = await service.AddImages(request);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    public async Task<IActionResult> Get()
    {
        return Ok();
    }
}