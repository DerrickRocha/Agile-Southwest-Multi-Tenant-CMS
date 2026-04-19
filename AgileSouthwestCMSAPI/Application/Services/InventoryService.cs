using System.ComponentModel.DataAnnotations;
using AgileSouthwestCMSAPI.Api.Requests.inventory;
using AgileSouthwestCMSAPI.Application.DTOs.Inventory;
using AgileSouthwestCMSAPI.Application.Exceptions;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public class InventoryService(ITenantContext context, CmsDbContext database, bool skipTransactionsForTesting = false)
    : IInventoryService
{
    public async Task<AddToInventoryResult> AddToInventory(AddItemToInventoryRequest request)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");
        var quantity = request.Quantity ?? throw new ValidationException("Quantity cannot be null");
        if (quantity < 0)
        {
            throw new ValidationException("quantity cannot be negative.");
        }

        if (request.ProductId == null)
        {
            throw new ValidationException("productId cannot be null");
        }

        if (request.StoreId == null)
        {
            throw new ValidationException("storeId cannot be null.");
        }

        var store =
            await database.Stores.FirstOrDefaultAsync(s =>
                s.Id == request.StoreId && s.TenantId == tenant.Id && s.DeletedAt == null) ??
            throw new NotFoundException($"Store with ID {request.StoreId} not found");
        
        if (!store.IsOnline)
        {
            throw new ValidationException($"Store '{store.Name}' is not active");
        }

        var product = await database.Products.FirstOrDefaultAsync(p => p.Id == request.ProductId && p.TenantId == tenant.Id && p.DeletedAt == null) ??
                      throw new NotFoundException($"Product with ID {request.ProductId} not found");

        if (!product.IsActive)
        {
            throw new ValidationException($"Product '{product.Name}' is not active");
        }

        await using var transaction = await database.Database.BeginTransactionAsync();

        try
        {
            var rowsAffected = await database.Inventory
                .Where(i => i.TenantId == tenant.Id 
                            && i.StoreId == request.StoreId 
                            && i.ProductId == request.ProductId
                            && i.DeletedAt == null)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(i => i.Quantity, i => i.Quantity + request.Quantity)
                    .SetProperty(i => i.UpdatedAt, DateTime.UtcNow));

            if (rowsAffected == 0)
            {
                var item = new Inventory()
                {
                    Quantity = quantity,
                    Product = product,
                    Tenant = tenant,
                    Store = store
                };
                database.Inventory.Add(item);
                await database.SaveChangesAsync();
                await transaction.CommitAsync();
            
                return new AddToInventoryResult(item.Id);
            }
            
            var existingItem = await database.Inventory
                .FirstAsync(i => i.TenantId == tenant.Id 
                                 && i.StoreId == request.StoreId 
                                 && i.ProductId == request.ProductId);
    
            return new AddToInventoryResult(existingItem.Id);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            await transaction.RollbackAsync();
            throw new ConflictException("Inventory item already exists for this store and product");    
        }
        
    }

    public async Task<AddToInventoryResult> GetInventoryItem(int id)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");
        var item = await database.Inventory.FirstOrDefaultAsync(i => i.TenantId == tenant.Id && i.Id == id) ?? throw new NotFoundException("Inventory item not found.");
        return new AddToInventoryResult(item.Id);
    }
    
    private bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        return ex.InnerException?.Message?.Contains("Duplicate entry") == true ||
               ex.InnerException?.Message?.Contains("inventory_tenant_store_product_uk") == true;
    }
}