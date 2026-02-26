using AgileSouthwestCMSAPI.Application.DTOs.Tenants;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface ITenantsService
{
    Task<AddTenantResult> AddTenant(AddTenantRequest request);
    Task<GetTenantResult> GetTenant();
    Task<UpdateTenantResult> UpdateTenant(UpdateTenantRequest request);
    Task<GetTenantSubscriptionResult> GetTenantSubscription(GetTenantSubscriptionRequest request);
    Task<ChangeTenantSubscriptionResult> UpdateTenantSubscription(ChangeTenantSubsciptionRequest request);
}