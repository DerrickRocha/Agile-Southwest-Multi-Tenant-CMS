using AgileSouthwestCMSAPI.Api.Requests.Orders;
using AgileSouthwestCMSAPI.Application.DTOs.Orders;
using AgileSouthwestCMSAPI.Application.Interfaces;

namespace AgileSouthwestCMSAPI.Application.Services;

public class OrdersService: IOrderService
{
    public Task<CreateOrderResult> CreateOrder(CreateOrderRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<GetOrderResult> GetOrder(int id)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<GetOrderResult>> GetOrders(OrderQueryParameters parameters)
    {
        throw new NotImplementedException();
    }

    public Task<UpdateOrderResult> UpdateOrderStatus(int id, UpdateOrderStatusRequest request)
    {
        throw new NotImplementedException();
    }

    public Task CancelOrder(int id, string reason)
    {
        throw new NotImplementedException();
    }

    public Task<RefundResult> ProcessRefund(int id, RefundRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<OrderStatusHistoryResult>> GetOrderHistory(int id)
    {
        throw new NotImplementedException();
    }
}