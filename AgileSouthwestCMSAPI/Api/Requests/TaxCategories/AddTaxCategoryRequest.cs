namespace AgileSouthwestCMSAPI.Api.Requests.TaxCategories;

public record AddTaxCategoryRequest(string? Name, decimal? TaxRate);