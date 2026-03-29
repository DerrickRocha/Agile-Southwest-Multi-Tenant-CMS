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

        Assert.Equal("Base price must be greater than 0", ex.Message);
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
}