namespace AgileSouthwestCMSAPI.Application.DTOs.Orders;

public record OrderStatusHistoryResult(
    int Id,
    int OrderId,
    string OldStatus,
    string NewStatus,
    string OldPaymentStatus,
    string NewPaymentStatus,
    string OldFulfillmentStatus,
    string NewFulfillmentStatus,
    int ChangedBy,
    string Reason,
    DateTime CreatedAt
);