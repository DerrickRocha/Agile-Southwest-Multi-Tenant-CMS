using AgileSouthwestCMSAPI.Api.Requests.Orders;
using AgileSouthwestCMSAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AgileSouthwestCMSAPI.Api.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class OrdersController(IOrderService service) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var result = await service.CreateOrder(request);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await service.GetOrder(id);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] OrderQueryParameters parameters)
    {
        var results = await service.GetOrders(parameters);
        return Ok(results);
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
    {
        var result = await service.UpdateOrderStatus(id, request);
        return Ok(result);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<IActionResult> Cancel(int id, [FromBody] CancelOrderRequest request)
    {
        await service.CancelOrder(id, request.Reason);
        return NoContent();
    }

    [HttpPost("{id:int}/refund")]
    public async Task<IActionResult> Refund(int id, [FromBody] RefundRequest request)
    {
        var result = await service.ProcessRefund(id, request);
        return Ok(result);
    }

    [HttpGet("{id:int}/history")]
    public async Task<IActionResult> GetHistory(int id)
    {
        var results = await service.GetOrderHistory(id);
        return Ok(results);
    }
}