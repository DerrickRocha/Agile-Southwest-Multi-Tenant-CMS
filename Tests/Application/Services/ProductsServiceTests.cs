using AgileSouthwestCMSAPI.Api.Requests.Products;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Application.Services;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.Application.Services;

public class ProductsServiceTests
{
    private CmsDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<CmsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CmsDbContext(options);
    }
    
    [Fact]
    public async Task CreateProduct_ThrowsUnauthorized_WhenTenantMissing()
    {

        await using var db = CreateDb();
        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns((Tenant?)null);

        var service = new ProductsService(tenantContext.Object, db);

        var request = new ProductRequest
        {
            Name = "Test",
            Description = "Test",
            BasePrice = 1000,
            IsActive = true,
            Options = []
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateProduct(request));
    }

    [Fact]
    public async Task CreateProduct_ThrowsInvalidOperation_WhenNoName()
    {
        await using var db = CreateDb();

        var tenant = new Tenant
        {
            Id = 1,
            Name = "Tenant",
            SubDomain = "tenant",
        };

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);
        
        var service = new ProductsService(tenantContext.Object, db);

        var request = new ProductRequest
        {
            Name = "",
            Description = "Test",
            BasePrice = 100,
            IsActive = true,
            Options = 
            [
                
            ]
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateProduct(request));
        
        Assert.Equal("Name is required.", ex.Message);
    }
    
    [Fact]
    public async Task CreateProduct_ThrowsInvalidOperation_WhenIsActiveRequiredAndMissing()
    {
        await using var db = CreateDb();

        var tenant = new Tenant
        {
            Id = 1,
            Name = "Tenant",
            SubDomain = "tenant",
        };

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);
        
        var service = new ProductsService(tenantContext.Object, db);

        var request = new ProductRequest
        {
            Name = "Test",
            Description = "Test",
            BasePrice = 100,
            Options = 
            [
                
            ]
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateProduct(request));
        
        Assert.Equal("IsActive is required.", ex.Message);
    }

    [Fact]
    public async Task CreateProduct_ThrowsInvalidOperation_WhenBasePriceRequiredAndMissing()
    {

        await using var db = CreateDb();

        var tenant = new Tenant
        {
            Id = 1,
            Name = "Tenant",
            SubDomain = "tenant",
        };

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);

        var service = new ProductsService(tenantContext.Object, db);

        var request = new ProductRequest
        {
            Name = "Test",
            Description = "Test",
            BasePrice = 0,
            IsActive = true,
            Options = 
            [
                new ProductOptionRequest
                {
                    Name = "Size",
                    Choices = 
                    [
                        new ProductOptionChoiceRequest()
                        {
                            Name = "Small",
                            PriceDelta = 0,
                            SalePriceDelta = 0
                        }
                    ]
                }
            ]
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateProduct(request));

        Assert.Equal("Base price must be greater than 0 if option choices don't have a price delta.", ex.Message);
    }
    
    [Fact]
    public async Task CreateProduct_ThrowsInvalidOperation_WhenOptionNameMissing()
    {

        await using var db = CreateDb();

        var tenant = new Tenant
        {
            Id = 1,
            Name = "Tenant",
            SubDomain = "tenant",
        };
        

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);

        var service = new ProductsService(tenantContext.Object, db);

        var request = new ProductRequest
        {
            Name = "Test",
            Description = "Test",
            BasePrice = 0,
            IsActive = true,
            Options = 
            [
                new ProductOptionRequest
                {
                    Name = "",
                    Choices = 
                    [
                        new ProductOptionChoiceRequest()
                        {
                            Name = "Small",
                            PriceDelta = 100,
                            SalePriceDelta = 0
                        }
                    ]
                }
            ]
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateProduct(request));

        Assert.Equal("Option name is required.", ex.Message);
    }
    
    [Fact]
    public async Task CreateProduct_ThrowsInvalidOperation_WhenOptionChoiceNameMissing()
    {

        await using var db = CreateDb();

        var tenant = new Tenant
        {
            Id = 1,
            Name = "Tenant",
            SubDomain = "tenant",
        };

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);

        var service = new ProductsService(tenantContext.Object, db);

        var request = new ProductRequest
        {
            Name = "Test",
            Description = "Test",
            BasePrice = 0,
            IsActive = true,
            Options = 
            [
                new ProductOptionRequest
                {
                    Name = "Size",
                    Choices = 
                    [
                        new ProductOptionChoiceRequest()
                        {
                            Name = "",
                            PriceDelta = 100,
                            SalePriceDelta = 0
                        }
                    ]
                }
            ]
        };

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateProduct(request));

        Assert.Equal("Option choice name is required.", ex.Message);
    }

    [Fact]
    public async Task CreateProduct_CreatesAndReturnsProduct()
    {

        await using var db = CreateDb();

        var tenant = new Tenant
        {
            Id = 1,
            Name = "Tenant",
            SubDomain = "tenant",
        };

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);

        var service = new ProductsService(tenantContext.Object, db, true);

        var request = new ProductRequest
        {
            Name = "Coffee",
            Description = "Fresh coffee",
            BasePrice = 1000,
            IsActive = true,
            Options = 
            [
                new ProductOptionRequest
                {
                    Name = "Size",
                    Choices = 
                    [
                        new ProductOptionChoiceRequest
                        {
                            Name = "Small",
                            PriceDelta = 100,
                            SalePriceDelta = 50
                        }
                    ]
                }
            ]
        };

        var result = await service.CreateProduct(request);

        Assert.NotNull(result);
        Assert.Equal("Coffee", result.Name);
        Assert.Equal("Fresh coffee", result.Description);
        Assert.Equal(1000, result.BasePrice);
        Assert.True(result.IsActive);

        var product = await db.Products
            .Include(p => p.ProductOptions)
            .ThenInclude(o => o.ProductOptionChoices)
            .SingleAsync();

        Assert.Equal(tenant.Id, product.TenantId);
        Assert.Equal("Coffee", product.Name);
        Assert.Single(product.ProductOptions);
        Assert.Single(product.ProductOptions.ToList()[0].ProductOptionChoices);
        Assert.Equal("Small", product.ProductOptions.ToList()[0].ProductOptionChoices.ToList()[0].Name);
    }
    
    [Fact]
    public async Task UpdateProduct_UpdatesProductAndReturnsResult()
    {
        await using var db = CreateDb();

        var tenant = new Tenant
        {
            Id = 1,
            Name = "Tenant",
            SubDomain = "tenant"
        };

        db.Products.Add(new Product
        {
            Id = 10,
            TenantId = tenant.Id,
            Name = "Old Name",
            Description = "Old Description",
            BasePriceCents = 500,
            IsActive = false,
            ProductOptions = []
        });
        await db.SaveChangesAsync();

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);

        var service = new ProductsService(tenantContext.Object, db);

        var request = new ProductRequest
        {
            Name = "New Name",
            Description = "New Description",
            BasePrice = 1000,
            IsActive = true,
            Options =
            [
                new ProductOptionRequest
                {
                    Name = "Size",
                    IsRequired = true,
                    Choices =
                    [
                        new ProductOptionChoiceRequest
                        {
                            Name = "Small",
                            PriceDelta = 0,
                            SalePriceDelta = 0,
                            IsActive = true
                        }
                    ]
                }
            ]
        };

        var result = await service.UpdateProduct(10, request);

        Assert.NotNull(result);
        Assert.Equal("New Name", result.Name);
        Assert.Equal("New Description", result.Description);
        Assert.Equal(1000, result.BasePrice);
        Assert.True(result.IsActive);

        var updatedProduct = await db.Products
            .Include(p => p.ProductOptions)
            .ThenInclude(o => o.ProductOptionChoices)
            .SingleAsync(p => p.Id == 10);

        Assert.Equal("New Name", updatedProduct.Name);
        Assert.Equal("New Description", updatedProduct.Description);
        Assert.Equal(1000, updatedProduct.BasePriceCents);
        Assert.True(updatedProduct.IsActive);
        Assert.Single(updatedProduct.ProductOptions);
        Assert.Single(updatedProduct.ProductOptions.First().ProductOptionChoices);
    }

    [Fact]
    public async Task UpdateProduct_ThrowsKeyNotFound_WhenProductMissing()
    {
        await using var db = CreateDb();

        var tenant = new Tenant
        {
            Id = 1,
            Name = "Tenant",
            SubDomain = "tenant"
        };

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);

        var service = new ProductsService(tenantContext.Object, db);

        var request = new ProductRequest
        {
            Name = "New Name",
            Description = "New Description",
            BasePrice = 1000,
            IsActive = true,
            Options = []
        };

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.UpdateProduct(999, request));

        Assert.Equal("Product not found.", ex.Message);
    }

    [Fact]
    public async Task GetProduct_ReturnsProductResult()
    {
        await using var db = CreateDb();

        var tenant = new Tenant
        {
            Id = 1,
            Name = "Tenant",
            SubDomain = "tenant"
        };

        db.Products.Add(new Product
        {
            Id = 10,
            TenantId = tenant.Id,
            Name = "Coffee",
            Description = "Fresh coffee",
            BasePriceCents = 1000,
            IsActive = true,
            ProductOptions =
            [
                new ProductOption
                {
                    Name = "Size",
                    IsRequired = true,
                    ProductOptionChoices =
                    [
                        new ProductOptionChoice
                        {
                            Name = "Small",
                            PriceDeltaCents = 0,
                            SalePriceDeltaCents = 0,
                            IsActive = true
                        }
                    ]
                }
            ]
        });
        await db.SaveChangesAsync();

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);

        var service = new ProductsService(tenantContext.Object, db);

        var result = await service.GetProduct(10);

        Assert.NotNull(result);
        Assert.Equal("Coffee", result.Name);
        Assert.Equal("Fresh coffee", result.Description);
        Assert.Equal(1000, result.BasePrice);
        Assert.True(result.IsActive);
        Assert.Single(result.ProductOptions);
        Assert.Single(result.ProductOptions.First().ProductOptionChoices);
    }

    [Fact]
    public async Task GetProduct_ThrowsInvalidOperation_WhenProductMissing()
    {
        await using var db = CreateDb();

        var tenant = new Tenant
        {
            Id = 1,
            Name = "Tenant",
            SubDomain = "tenant"
        };

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);

        var service = new ProductsService(tenantContext.Object, db);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetProduct(999));

        Assert.Equal("Product not found.", ex.Message);
    }
}