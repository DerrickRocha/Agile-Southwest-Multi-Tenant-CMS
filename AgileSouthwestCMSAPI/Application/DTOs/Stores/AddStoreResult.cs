namespace AgileSouthwestCMSAPI.Application.DTOs.Stores;

public record AddStoreResult(int Id, int TenantId, string Name, string SubDomain, bool IsOnline);