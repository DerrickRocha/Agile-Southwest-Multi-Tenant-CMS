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

        var service = new ProductsService(tenantContext.Object, db, true);

        var request = new ProductRequest
        {
            Name = "Test",
            Description = "Test",
            BasePrice = 1000,
            IsActive = true,
            Options = []
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.CreateProduct(request));
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

        var service = new ProductsService(tenantContext.Object, db, true);

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

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateProduct(request));

        Assert.Contains("Name is required.", ex.Message);
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

        var service = new ProductsService(tenantContext.Object, db, true);

        var request = new ProductRequest
        {
            Name = "Test",
            Description = "Test",
            BasePrice = 100,
            Options =
            [
            ]
        };

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateProduct(request));

        Assert.Contains("IsActive is required.", ex.Message);
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

        var service = new ProductsService(tenantContext.Object, db, true);

        var request = new ProductRequest
        {
            Name = "Test",
            Description = "Test",
            IsActive = true,
            Options =
            [
                new ProductOptionRequest
                {
                    Name = "Size",
                    IsRequired = true,
                    Choices =
                    [
                        new ProductOptionChoiceRequest()
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

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateProduct(request));

        Assert.Contains("Base price is required.", ex.Message);
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

        var service = new ProductsService(tenantContext.Object, db, true);

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

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateProduct(request));

        Assert.Contains("Option name is required.", ex.Message);
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

        var service = new ProductsService(tenantContext.Object, db, true);

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

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.CreateProduct(request));

        Assert.Contains("Option choice name is required.", ex.Message);
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
                    IsRequired = true,
                    Choices =
                    [
                        new ProductOptionChoiceRequest
                        {
                            Name = "Small",
                            PriceDelta = 100,
                            SalePriceDelta = 50,
                            IsActive = true
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

        var service = new ProductsService(tenantContext.Object, db, true);

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

        var service = new ProductsService(tenantContext.Object, db, true);

        var request = new ProductRequest
        {
            Name = "New Name",
            Description = "New Description",
            BasePrice = 1000,
            IsActive = true,
            Options = []
        };

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.UpdateProduct(999, request));

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

        var service = new ProductsService(tenantContext.Object, db, true);

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

        var service = new ProductsService(tenantContext.Object, db, true);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.GetProduct(999));

        Assert.Equal("Product not found.", ex.Message);
    }

    [Fact]
    public async Task PatchProduct_UpdatesScalarFields_WhenProvided()
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

        var service = new ProductsService(tenantContext.Object, db, true);

        var request = new PatchProductRequest
        {
            Name = "New Name",
            Description = "New Description",
            BasePrice = 1000,
            IsActive = true
        };

        var result = await service.PatchProduct(10, request);

        Assert.Equal("New Name", result.Name);
        Assert.Equal("New Description", result.Description);
        Assert.Equal(1000, result.BasePrice);
        Assert.True(result.IsActive);

        var updated = await db.Products.SingleAsync(p => p.Id == 10);
        Assert.Equal("New Name", updated.Name);
        Assert.Equal("New Description", updated.Description);
        Assert.Equal(1000, updated.BasePriceCents);
        Assert.True(updated.IsActive);
    }

    [Fact]
    public async Task PatchProduct_UpdatesOptions_WhenProvided()
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
            Name = "Product",
            Description = "Description",
            BasePriceCents = 500,
            IsActive = true,
            ProductOptions =
            [
                new ProductOption
                {
                    Name = "Old Option",
                    IsRequired = false,
                    ProductOptionChoices =
                    [
                        new ProductOptionChoice
                        {
                            Name = "Old Choice",
                            PriceDeltaCents = 10,
                            SalePriceDeltaCents = 5,
                            IsActive = true
                        }
                    ]
                }
            ]
        });

        await db.SaveChangesAsync();

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);

        var service = new ProductsService(tenantContext.Object, db, true);

        var request = new PatchProductRequest
        {
            Options =
            [
                new PatchProductOptionRequest
                {
                    Name = "New Option",
                    IsRequired = true,
                    Choices =
                    [
                        new PatchProductOptionChoiceRequest
                        {
                            Name = "New Choice",
                            PriceDelta = 0,
                            SalePriceDelta = 0,
                            IsActive = true
                        }
                    ]
                }
            ]
        };

        var result = await service.PatchProduct(10, request);

        Assert.Single(result.ProductOptions);
        Assert.Equal("New Option", result.ProductOptions[0].Name);
        Assert.True(result.ProductOptions[0].IsRequired);
        Assert.Single(result.ProductOptions[0].ProductOptionChoices);
        Assert.Equal("New Choice", result.ProductOptions[0].ProductOptionChoices[0].Name);

        var updated = await db.Products
            .Include(p => p.ProductOptions)
            .ThenInclude(o => o.ProductOptionChoices)
            .SingleAsync(p => p.Id == 10);

        Assert.Single(updated.ProductOptions);
        Assert.Equal("New Option", updated.ProductOptions.First().Name);
        Assert.Single(updated.ProductOptions.First().ProductOptionChoices);
    }

    [Fact]
    public async Task PatchProduct_LeavesUnspecifiedFieldsUnchanged()
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

        var service = new ProductsService(tenantContext.Object, db, true);

        var request = new PatchProductRequest
        {
            Name = "Only Name Changed"
        };

        var result = await service.PatchProduct(10, request);

        Assert.Equal("Only Name Changed", result.Name);
        Assert.Equal("Old Description", result.Description);
        Assert.Equal(500, result.BasePrice);
        Assert.False(result.IsActive);
    }

    [Fact]
    public async Task PatchProduct_ThrowsKeyNotFound_WhenProductMissing()
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

        var service = new ProductsService(tenantContext.Object, db, true);

        var request = new PatchProductRequest
        {
            Name = "Changed"
        };

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.PatchProduct(999, request));

        Assert.Equal("Product not found.", ex.Message);
    }

    [Fact]
    public async Task PatchProduct_ThrowsUnauthorized_WhenTenantMissing()
    {
        await using var db = CreateDb();

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns((Tenant?)null);

        var service = new ProductsService(tenantContext.Object, db, true);

        var request = new PatchProductRequest
        {
            Name = "Changed"
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.PatchProduct(10, request));
    }
}