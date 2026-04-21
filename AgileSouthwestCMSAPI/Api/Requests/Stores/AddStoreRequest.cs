namespace AgileSouthwestCMSAPI.Api.Requests.Stores;

public record AddStoreRequest(string Name, string SubDomain, bool? IsOnline);