namespace AgileSouthwestCMSAPI.Api.Requests.Orders;

public record OrderRequestItem(int ProductId, int Quantity, Dictionary<int, int>? SelectedOptions);