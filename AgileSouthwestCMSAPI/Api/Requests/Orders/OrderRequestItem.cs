namespace AgileSouthwestCMSAPI.Api.Requests.Orders;

public record OrderRequestItem(int ProductId, int Quantity, Dictionary<string, string>? SelectedOptions);