using AgileSouthwestCMSAPI.Api.Requests.Orders;
using AgileSouthwestCMSAPI.Application.DTOs.Orders;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface IOrderService
{
    Task<CreateOrderResult> CreateOrder(CreateOrderRequest request);
    Task<GetOrderResult> GetOrder(int id);
    Task<IEnumerable<GetOrderResult>> GetOrders(OrderQueryParameters parameters);
    Task<UpdateOrderResult> UpdateOrderStatus(int id, UpdateOrderStatusRequest request);
    Task CancelOrder(int id, string reason);
    Task<RefundResult> ProcessRefund(int id, RefundRequest request);
    Task<IEnumerable<OrderStatusHistoryResult>> GetOrderHistory(int id);
}