using AgileSouthwestCMSAPI.Api.Requests.inventory;
using AgileSouthwestCMSAPI.Api.Requests.Stores;
using AgileSouthwestCMSAPI.Application.DTOs.Stores;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface IStoresService
{
    public Task<AddStoreResult> AddStore(AddStoreRequest request);
    public Task<AddStoreResult> GetStore(int id);
}