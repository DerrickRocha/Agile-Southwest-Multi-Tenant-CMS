namespace AgileSouthwestCMSAPI.Api.Requests.inventory;

public record AddItemToInventoryRequest(int? ProductId, int? StoreId, int? Quantity);