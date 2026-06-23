using AgileSouthwestCMSAPI.Api.Requests.TaxCategories;
using AgileSouthwestCMSAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class TaxCategoriesController(ITaxCategoriesService service): ControllerBase
{

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] AddTaxCategoryRequest request)
    {
        var result = await service.AddTaxCategory(request);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await service.GetTaxCategory(id);
        return Ok(result);
    }
}