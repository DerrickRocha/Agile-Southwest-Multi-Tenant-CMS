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
        
        if (request.Quantity < 0)
        {
            throw new ValidationException("Quantity cannot be negative.");
        }

        var store =
            database.Stores.FirstOrDefault(s =>
                s.Id == request.StoreId && s.TenantId == tenant.Id && s.DeletedAt == null) ??
            throw new NotFoundException($"Store with ID {request.StoreId} not found");
        
        if (!store.IsOnline)
        {
            throw new ValidationException($"Store '{store.Name}' is not active");
        }

        var product = database.Products.FirstOrDefault(p => p.Id == request.ProductId && p.TenantId == tenant.Id) ??
                      throw new NotFoundException($"Product with ID {request.ProductId} not found");

        if (!product.IsActive)
        {
            throw new ValidationException($"Product '{product.Name}' is not active");
        }
        
        var existingInventory = await database.Inventory
            .FirstOrDefaultAsync(i => i.StoreId == request.StoreId 
                                      && i.ProductId == request.ProductId 
                                      && i.TenantId == tenant.Id
                                      && i.DeletedAt == null);
    
        if (existingInventory != null)
        {
            // Update existing inventory
            existingInventory.Quantity += request.Quantity;
            await database.SaveChangesAsync();
            return new AddToInventoryResult(existingInventory.Id);
        }
        
        var item = new Inventory()
        {
            Quantity = request.Quantity,
            Product = product,
            Tenant = tenant,
            Store = store
        };
        database.Inventory.Add(item);
        await database.SaveChangesAsync();
        return new AddToInventoryResult(item.Id);
    }

    public async Task<AddToInventoryResult> GetInventoryItem(int id)
    {
        var tenant = context.Tenant
                     ?? throw new UnauthorizedAccessException("Tenant not resolved.");
        var item = await database.Inventory.FirstOrDefaultAsync(i => i.TenantId == tenant.Id && i.Id == id) ?? throw new NotFoundException("Inventory item not found.");
        return new AddToInventoryResult(item.Id);
    }
}