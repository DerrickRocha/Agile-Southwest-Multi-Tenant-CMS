using AgileSouthwestCMSAPI.Application.DTOs.Tenants;
using AgileSouthwestCMSAPI.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
[Authorize]
public class TenantController(ITenantsService service): ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetTenant(GetTenantRequest request)
    {
        var result = await service.GetTenant(request);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateTenant([FromBody] UpdateTenantRequest request)
    {
        var result = await service.UpdateTenant(request);
        return Ok(result);
    }

    [HttpGet("subscription")]
    public async Task<IActionResult> GetSubscription([FromBody] GetTenantSubscriptionRequest request)
    {
        var result = await service.GetTenantSubscription(request);
        return Ok(result);
    }

    [HttpPost("subscription/change")]
    public async Task<IActionResult> UpdateSubscription([FromBody] ChangeTenantSubsciptionRequest request)
    {
        return Ok(await service.UpdateTenantSubscription(request));
    }
    
}