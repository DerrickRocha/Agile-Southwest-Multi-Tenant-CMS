using AgileSouthwestCMSAPI.Api.Requests.inventory;
using AgileSouthwestCMSAPI.Application.DTOs.Inventory;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface IInventoryService
{
    public Task<AddToInventoryResult> AddToInventory(AddItemToInventoryRequest request);
    public Task<AddToInventoryResult> GetInventoryItem(int id);
}