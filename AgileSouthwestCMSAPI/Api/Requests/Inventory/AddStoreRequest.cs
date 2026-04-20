namespace AgileSouthwestCMSAPI.Api.Requests.inventory;

public record AddStoreRequest(string Name, string SubDomain, bool? IsOnline);