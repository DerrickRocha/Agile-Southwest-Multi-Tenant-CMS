using AgileSouthwestCMSAPI.Api.Requests.TaxCategories;
using AgileSouthwestCMSAPI.Application.DTOs.TaxCategories;

namespace AgileSouthwestCMSAPI.Application.Interfaces;

public interface ITaxCategoriesService
{
    public Task<TaxCategoryResult> AddTaxCategory(AddTaxCategoryRequest request);
    
    public Task<TaxCategoryResult> GetTaxCategory(int id);
}