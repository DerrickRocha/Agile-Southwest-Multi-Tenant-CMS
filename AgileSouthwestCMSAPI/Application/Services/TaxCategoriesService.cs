using System.ComponentModel.DataAnnotations;
using AgileSouthwestCMSAPI.Api.Requests.TaxCategories;
using AgileSouthwestCMSAPI.Application.DTOs.TaxCategories;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public class TaxCategoriesService(ITenantContext tenantContext, CmsDbContext database): ITaxCategoriesService
{
    public async Task<TaxCategoryResult> AddTaxCategory(AddTaxCategoryRequest request)
    {
        var tenant = tenantContext.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved");
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new ValidationException("Tax category name cannot be null or whitespace");
        }

        if (request.TaxRate is null)
        {
            throw new ValidationException("Tax rate cannot be null");
        }
        var category = new TaxCategory
        {
            Tenant = tenant,
            Name = request.Name,
            TaxRate = request.TaxRate ?? 0
        };
        database.TaxCategories.Add(category);
        await database.SaveChangesAsync();
        return new TaxCategoryResult(category.Id, category.Name, category.TaxRate);
    }

    public async Task<TaxCategoryResult> GetTaxCategory(int id)
    {
        var tenant = tenantContext.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved");
        var category = await database.TaxCategories.FirstOrDefaultAsync(c => c.Id == id && c.TenantId == tenant.Id) ?? throw new InvalidOperationException("Tax category not found");
        return new TaxCategoryResult(category.Id, category.Name, category.TaxRate);
    }
}