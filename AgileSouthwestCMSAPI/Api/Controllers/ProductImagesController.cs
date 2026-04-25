using AgileSouthwestCMSAPI.Api.Requests.ProductImages;
using AgileSouthwestCMSAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class ProductImagesController(IProductImagesService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AttachImageToProductRequest request)
    {
        var result = await service.AttachImageToProduct(request);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await service.GetProductImage(id);
        return Ok(result);
    }

    [HttpGet("product/{productId:int}")]
    public async Task<IActionResult> GetByProduct(int productId)
    {
        var results = await service.GetProductImagesByProductId(productId);
        return Ok(results);
    }

    [HttpGet("image/{imageId:int}")]
    public async Task<IActionResult> GetByImage(int imageId)
    {
        var results = await service.GetProductImagesByImageId(imageId);
        return Ok(results);
    }

    [HttpPut("{id:int}/primary")]
    public async Task<IActionResult> SetAsPrimary(int id)
    {
        await service.SetAsPrimary(id);
        return NoContent();
    }

    [HttpPut("{id:int}/position")]
    public async Task<IActionResult> UpdatePosition(int id, [FromBody] UpdatePositionRequest request)
    {
        await service.UpdatePosition(id, request.Position);
        return NoContent();
    }

    [HttpPost("reorder")]
    public async Task<IActionResult> ReorderImages([FromBody] ReorderImagesRequest request)
    {
        await service.ReorderImages(request);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DetachImageFromProduct(id);
        return NoContent();
    }

    [HttpDelete("product/{productId:int}/image/{imageId:int}")]
    public async Task<IActionResult> DetachByProductAndImage(int productId, int imageId)
    {
        await service.DetachImageFromProductByKeys(productId, imageId);
        return NoContent();
    }
}