using AgileSouthwestCMSAPI.Application.DTOs.Products;
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
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var result = await service.CreateProduct(request);
        return Ok(result);
    }
}