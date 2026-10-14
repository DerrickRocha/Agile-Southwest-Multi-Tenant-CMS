namespace AgileSouthwestCMSAPI.Api.Requests.Orders;

public sealed record OrderItemOptionSnapshot(string OptionName, string ChoiceName, int PriceDeltaCents);