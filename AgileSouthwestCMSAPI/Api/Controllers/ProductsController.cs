using AgileSouthwestCMSAPI.Api.Requests.Products;
using AgileSouthwestCMSAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[Authorize(Policy = "TenantAdmin")]
public class ProductsController(IProductsService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductRequest request)
    {
        var result = await service.CreateProduct(request);
        return CreatedAtAction(
            nameof(Get),
            new { id = result.Id },
            result
        );
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await service.GetProduct(id);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] GetProductsQuery query)
    {
        var result = await service.GetProducts(query);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteProduct(id);
        return NoContent();
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductRequest request)
    {
        var result = await service.UpdateProduct(id, request);
        return Ok(result);
    }

    [HttpPatch("{id:int}")]
    public async Task<IActionResult> Patch(int id, [FromBody] PatchProductRequest request)
    {
        var result = await service.PatchProduct(id, request);
        return Ok(result);
        
    }
    
}