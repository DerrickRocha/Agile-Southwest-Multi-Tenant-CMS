using AgileSouthwestCMSAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ImagesController(IImagesService service): ControllerBase
{
    
    [HttpPost]
    public async Task<IActionResult> Post(IFormFileCollection files)
    {
        var result = await service.AddImages(files);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    public async Task<IActionResult> Get()
    {
        return Ok();
    }
}