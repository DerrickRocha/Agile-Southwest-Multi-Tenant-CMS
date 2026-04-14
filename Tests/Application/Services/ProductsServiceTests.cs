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
    
    [Fact]
    public async Task GetProducts_ReturnsOnlyCurrentTenantsActiveProducts()
    {
        await using var db = CreateDb();

        var tenant = new Tenant { Id = 1, Name = "Tenant", SubDomain = "tenant" };
        var otherTenant = new Tenant { Id = 2, Name = "Other", SubDomain = "other" };

        db.Products.AddRange(
            new Product
            {
                Id = 1,
                TenantId = tenant.Id,
                Name = "Coffee",
                Description = "Fresh coffee",
                BasePriceCents = 1000,
                IsActive = true
            },
            new Product
            {
                Id = 3,
                TenantId = tenant.Id,
                Name = "Coffee",
                Description = "Fresh coffee",
                BasePriceCents = 1000,
                IsActive = false
            },
            new Product
            {
                Id = 2,
                TenantId = otherTenant.Id,
                Name = "Milk",
                Description = "Whole milk",
                BasePriceCents = 500,
                IsActive = true
            });

        await db.SaveChangesAsync();

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);

        var service = new ProductsService(tenantContext.Object, db);

        var result = await service.GetProducts(new GetProductsQuery());

        Assert.Equal(1, result.TotalCount);
        Assert.Single(result.Items);
        Assert.Equal("Coffee", result.Items[0].Name);
    }

    [Fact]
    public async Task GetProducts_FiltersBySearch()
    {
        await using var db = CreateDb();

        var tenant = new Tenant { Id = 1, Name = "Tenant", SubDomain = "tenant" };

        db.Products.AddRange(
            new Product
            {
                Id = 1,
                TenantId = tenant.Id,
                Name = "Coffee",
                Description = "Fresh coffee",
                BasePriceCents = 1000,
                IsActive = true
            },
            new Product
            {
                Id = 2,
                TenantId = tenant.Id,
                Name = "Tea",
                Description = "Green tea",
                BasePriceCents = 700,
                IsActive = true
            });

        await db.SaveChangesAsync();

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);

        var service = new ProductsService(tenantContext.Object, db);

        var result = await service.GetProducts(new GetProductsQuery(Search: "coffee"));

        Assert.Single(result.Items);
        Assert.Equal("Coffee", result.Items[0].Name);
    }

    [Fact]
    public async Task GetProducts_FiltersByIsActive()
    {
        await using var db = CreateDb();

        var tenant = new Tenant { Id = 1, Name = "Tenant", SubDomain = "tenant" };

        db.Products.AddRange(
            new Product
            {
                Id = 1,
                TenantId = tenant.Id,
                Name = "Active Product",
                Description = "Active",
                BasePriceCents = 1000,
                IsActive = true
            },
            new Product
            {
                Id = 2,
                TenantId = tenant.Id,
                Name = "Inactive Product",
                Description = "Inactive",
                BasePriceCents = 500,
                IsActive = false
            });

        await db.SaveChangesAsync();

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);

        var service = new ProductsService(tenantContext.Object, db);

        var result = await service.GetProducts(new GetProductsQuery(IsActive: true));

        Assert.Single(result.Items);
        Assert.Equal("Active Product", result.Items[0].Name);
    }

    [Fact]
    public async Task GetProducts_UsesPagination()
    {
        await using var db = CreateDb();

        var tenant = new Tenant { Id = 1, Name = "Tenant", SubDomain = "tenant" };

        for (var i = 1; i <= 5; i++)
        {
            db.Products.Add(new Product
            {
                Id = i,
                TenantId = tenant.Id,
                Name = $"Product {i}",
                Description = $"Description {i}",
                BasePriceCents = i * 100,
                IsActive = true
            });
        }

        await db.SaveChangesAsync();

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);

        var service = new ProductsService(tenantContext.Object, db);

        var result = await service.GetProducts(new GetProductsQuery(Page: 2, PageSize: 2));

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Product 3", result.Items[0].Name);
        Assert.Equal("Product 4", result.Items[1].Name);
    }

    [Fact]
    public async Task GetProducts_ThrowsArgumentException_WhenPageIsInvalid()
    {
        await using var db = CreateDb();

        var tenant = new Tenant { Id = 1, Name = "Tenant", SubDomain = "tenant" };

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);

        var service = new ProductsService(tenantContext.Object, db);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.GetProducts(new GetProductsQuery(Page: 0, PageSize: 20)));
    }

    [Fact]
    public async Task GetProducts_ThrowsUnauthorized_WhenTenantMissing()
    {
        await using var db = CreateDb();

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns((Tenant?)null);

        var service = new ProductsService(tenantContext.Object, db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.GetProducts(new GetProductsQuery()));
    }
    
    [Fact]
    public async Task DeleteProduct_DeletesProduct()
    {
        var tenant = new Tenant { Id = 1, Name = "Tenant", SubDomain = "tenant" };

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);
        await using var db = CreateDb();
        var product = new Product { Id = 1, TenantId = tenant.Id, Name = "Product", BasePriceCents = 100, IsActive = true, Description = "Description", ProductOptions = new List<ProductOption>()};
        db.Products.Add(product);
        await db.SaveChangesAsync();
        
        var service = new ProductsService(tenantContext.Object, db, true);
        await service.DeleteProduct(1);
        
        var deletedProduct = await db.Products.SingleAsync(p => p.Id == 1);
        Assert.NotNull(deletedProduct.DeletedAt);
    }
    
    [Fact]
    public async Task DeleteProduct_ThrowsKeyNotFound_WhenProductMissing() {
        var tenant = new Tenant { Id = 1, Name = "Tenant", SubDomain = "tenant" };
        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);
        await using var db = CreateDb();
        var product = new Product { Id = 2, TenantId = tenant.Id, Name = "Product", BasePriceCents = 100, IsActive = true, Description = "Description", ProductOptions = new List<ProductOption>()};
        db.Products.Add(product);
        await db.SaveChangesAsync();
        
        var service = new ProductsService(tenantContext.Object, db, true);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.DeleteProduct(1));
    }
    
    [Fact]
    public async Task DeleteProduct_ThrowsUnauthorized_WhenTenantMissing() {
        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns((Tenant?)null);
        await using var db = CreateDb();
        var product = new Product { Id = 1, TenantId = 1, Name = "Product", BasePriceCents = 100, IsActive = true, Description = "Description", ProductOptions = new List<ProductOption>()};
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => new ProductsService(tenantContext.Object, db, true).DeleteProduct(1));
    }
    
    [Fact]
    public async Task DeleteProduct_ThrowsInvalidOperationException_WhenProductIsDeleted() {
        var tenant = new Tenant { Id = 1, Name = "Tenant", SubDomain = "tenant" };
        var tenantContext = new Mock<ITenantContext>();
        tenantContext.SetupGet(x => x.Tenant).Returns(tenant);
        await using var db = CreateDb();
        var product = new Product { Id = 1, TenantId = tenant.Id, Name = "Product", BasePriceCents = 100, IsActive = true, Description = "Description", ProductOptions = new List<ProductOption>(), DeletedAt = DateTime.Now};
        db.Products.Add(product);
        await db.SaveChangesAsync();
        
        var service = new ProductsService(tenantContext.Object, db, true);
        await Assert.ThrowsAsync<InvalidOperationException>(() => service.DeleteProduct(1));   
    }
}