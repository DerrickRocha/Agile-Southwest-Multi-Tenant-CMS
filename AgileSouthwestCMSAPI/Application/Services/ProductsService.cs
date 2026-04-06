using AgileSouthwestCMSAPI.Api.Requests.Products;
using AgileSouthwestCMSAPI.Application.DTOs.Products;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Extensions;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public class ProductsService(ITenantContext context, CmsDbContext database, bool skipTransactionsForTesting = false)
    : IProductsService
{
    public async Task<ProductResult> CreateProduct(ProductRequest request)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");

        ValidateProductRequest(request);

        var strategy = database.Database.CreateExecutionStrategy();

        if (skipTransactionsForTesting)
            return await WriteProduct(request, tenant.Id);

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await database.Database.BeginTransactionAsync();

            try
            {
                var result = await WriteProduct(request, tenant.Id);
                await transaction.CommitAsync();
                return result;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }

    public async Task<ProductResult> UpdateProduct(int id, ProductRequest request)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");

        ValidateProductRequest(request);

        var product = await database.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenant.Id);

        if (product is null)
            throw new KeyNotFoundException("Product not found.");

        product.Name = request.Name!;
        product.Description = request.Description!;
        product.BasePriceCents = request.BasePrice!.Value;
        product.IsActive = request.IsActive!.Value;
        product.ProductOptions = MapToProductOptions(request.Options!);

        await database.SaveChangesAsync();

        return product.ToProductResult();
    }

    public async Task<ProductResult> GetProduct(int id)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");

        var product = await database.Products
            .Include(p => p.ProductOptions)
            .ThenInclude(po => po.ProductOptionChoices)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenant.Id);
        return product == null ? throw new InvalidOperationException("Product not found.") : product.ToProductResult();
    }

    public async Task DeleteProduct(int id)
    {
        var tenant = context.Tenant ?? throw new UnauthorizedAccessException("Tenant not resolved.");
        var strategy = database.Database.CreateExecutionStrategy();
        if (skipTransactionsForTesting)
        {
            await DeleteProductFromDb(id, tenant.Id);
        }
        else
        {
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await database.Database.BeginTransactionAsync();
                try
                {
                    await DeleteProductFromDb(id, tenant.Id);
                    await transaction.CommitAsync();
                } catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            });  
        }
    }

    private async Task DeleteProductFromDb(int id, int tenantId)
    {
        var product = await database.Products
            .Include(p =>  p.ProductOptions)
            .ThenInclude(po => po.ProductOptionChoices)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId);
        if (product is null) throw new KeyNotFoundException("Product not found.");
        if (product.IsDeleted) throw new InvalidOperationException("Product already deleted.");
        
        var now = DateTime.UtcNow;
        
        product.IsDeleted = true;
        product.DeletedAt = now;

        foreach (var option in product.ProductOptions)
        {
            option.IsDeleted = true;
            option.DeletedAt = now;
            foreach (var choice in option.ProductOptionChoices)
            {
                choice.IsDeleted = true;
                choice.DeletedAt = now;
            }
        }
        await database.SaveChangesAsync();
    }

    public async Task<PagedResult<ProductListItemResult>> GetProducts(GetProductsQuery query)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");

        if (query.Page < 1)
            throw new ArgumentException("Page must be greater than 0.", nameof(query.Page));

        if (query.PageSize < 1)
            throw new ArgumentException("Page size must be greater than 0.", nameof(query.PageSize));

        const int maxPageSize = 100;
        var pageSize = Math.Min(query.PageSize, maxPageSize);

        var products = database.Products
            .AsNoTracking()
            .Where(p => p.TenantId == tenant.Id);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();

            products = products.Where(p =>
                p.Name.Contains(search) ||
                p.Description.Contains(search));
        }

        if (query.IsActive is not null)
        {
            products = products.Where(p => p.IsActive == query.IsActive.Value);
        }

        var totalCount = await products.CountAsync();

        var items = await products
            .OrderBy(p => p.Name)
            .ThenBy(p => p.Id)
            .Skip((query.Page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListItemResult(
                p.Id,
                p.Name,
                p.BasePriceCents,
                p.IsActive))
            .ToListAsync();

        return new PagedResult<ProductListItemResult>(
            items,
            query.Page,
            pageSize,
            totalCount);
    }

    public async Task<ProductResult> PatchProduct(int id, PatchProductRequest request)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");

        ArgumentNullException.ThrowIfNull(request);

        var product = await database.Products
            .Include(p => p.ProductOptions)
            .ThenInclude(o => o.ProductOptionChoices)
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenant.Id);

        if (product is null)
            throw new KeyNotFoundException("Product not found.");

        if (request.Name is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                throw new ArgumentException("Name cannot be empty.", nameof(request));

            product.Name = request.Name;
        }

        if (request.Description is not null)
        {
            if (string.IsNullOrWhiteSpace(request.Description))
                throw new ArgumentException("Description cannot be empty.", nameof(request));

            product.Description = request.Description;
        }

        if (request.BasePrice is not null)
        {
            if (request.BasePrice < 0)
                throw new ArgumentException("Base price cannot be negative.", nameof(request));

            product.BasePriceCents = request.BasePrice.Value;
        }

        if (request.IsActive is not null)
        {
            product.IsActive = request.IsActive.Value;
        }

        if (request.Options is not null)
        {
            product.ProductOptions = request.Options.Select(option =>
            {
                if (option is null)
                    throw new ArgumentException("Options cannot contain null items.", nameof(request));

                if (string.IsNullOrWhiteSpace(option.Name))
                    throw new ArgumentException("Option name is required.", nameof(request));

                if (option.Choices is null)
                    throw new ArgumentException("Choices are required for each option.", nameof(request));

                if (option.Choices.Length == 0)
                    throw new ArgumentException("Option must have at least one choice.", nameof(request));

                var choices = option.Choices.Select(choice =>
                {
                    if (choice is null)
                        throw new ArgumentException("Choices cannot contain null items.", nameof(request));

                    if (string.IsNullOrWhiteSpace(choice.Name))
                        throw new ArgumentException("Option choice name is required.", nameof(request));

                    if (choice.PriceDelta is null)
                        throw new ArgumentException("PriceDelta is required.", nameof(request));

                    if (choice.SalePriceDelta is null)
                        throw new ArgumentException("SalePriceDelta is required.", nameof(request));

                    if (choice.IsActive is null)
                        throw new ArgumentException("Option choice IsActive is required.", nameof(request));

                    return new ProductOptionChoice
                    {
                        Name = choice.Name,
                        PriceDeltaCents = choice.PriceDelta.Value,
                        SalePriceDeltaCents = choice.SalePriceDelta.Value,
                        IsActive = choice.IsActive.Value
                    };
                }).ToList();

                return new ProductOption
                {
                    Name = option.Name,
                    IsRequired = option.IsRequired ?? false,
                    ProductOptionChoices = choices
                };
            }).ToList();
        }

        await database.SaveChangesAsync();

        return product.ToProductResult();
    }

    private static void ValidateProductRequest(ProductRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.", nameof(request));

        if (string.IsNullOrWhiteSpace(request.Description))
            throw new ArgumentException("Description is required.", nameof(request));

        if (request.BasePrice is null)
            throw new ArgumentException("Base price is required.", nameof(request));

        if (request.IsActive is null)
            throw new ArgumentException("IsActive is required.", nameof(request));

        var options = request.Options ?? [];

        if (options.Any(o => o is null))
            throw new ArgumentException("Options cannot contain null items.", nameof(request));

        foreach (var option in options)
        {
            if (string.IsNullOrWhiteSpace(option!.Name))
                throw new ArgumentException("Option name is required.", nameof(request));

            var choices = option.Choices ?? [];

            if (choices.Any(c => c is null))
                throw new ArgumentException("Choices cannot contain null items.", nameof(request));

            if (choices.Length == 0)
                throw new ArgumentException("Option must have at least one choice.", nameof(request));

            foreach (var choice in choices)
            {
                if (string.IsNullOrWhiteSpace(choice!.Name))
                    throw new ArgumentException("Option choice name is required.", nameof(request));

                if (choice.PriceDelta is null)
                    throw new ArgumentException("PriceDelta is required.", nameof(request));

                if (choice.SalePriceDelta is null)
                    throw new ArgumentException("SalePriceDelta is required.", nameof(request));

                if (choice.IsActive is null)
                    throw new ArgumentException("Option choice IsActive is required.", nameof(request));
            }
        }
    }

    private static ProductOption[] MapToProductOptions(ProductOptionRequest[] options)
    {
        return options.Select(option => new ProductOption
        {
            Name = option!.Name!,
            IsRequired = option.IsRequired!.Value,
            ProductOptionChoices = (option.Choices ?? []).Select(choice => new ProductOptionChoice
            {
                Name = choice!.Name!,
                PriceDeltaCents = choice.PriceDelta!.Value,
                SalePriceDeltaCents = choice.SalePriceDelta!.Value,
                IsActive = choice.IsActive!.Value
            }).ToArray()
        }).ToArray();
    }

    private async Task<ProductResult> WriteProduct(ProductRequest request, int tenantId)
    {
        var product = new Product
        {
            TenantId = tenantId,
            Name = request.Name!,
            Description = request.Description!,
            BasePriceCents = request.BasePrice!.Value,
            IsActive = request.IsActive!.Value,
            ProductOptions = MapToProductOptions(request.Options!)
        };

        database.Products.Add(product);
        await database.SaveChangesAsync();

        return product.ToProductResult();
    }
}