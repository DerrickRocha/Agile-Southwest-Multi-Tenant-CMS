using AgileSouthwestCMSAPI.Application.DTOs.Tenants;
using AgileSouthwestCMSAPI.Application.Exceptions;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Application.Services;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Domain.Enums;
using AgileSouthwestCMSAPI.Domain.ValueObjects;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Tests.Application.Services;

public class TenantsServiceTests
{
    
    private CmsDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<CmsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CmsDbContext(options);
    }
    
    [Fact]
    public async Task AddTenant_CreatesTenantAndMembership()
    {
        var db = CreateDb();

        var user = new CmsUser
        {
            Id = 1,
            CognitoUserId = "abc",
            Email = "drocha"
        };

        db.CmsUsers.Add(user);
        await db.SaveChangesAsync();

        var tenantContext = new Mock<ITenantContext>();
        var userContext = new Mock<ICmsUserContext>();

        userContext.Setup(u => u.UserId).Returns("abc");

        var service = new TenantsService(db, tenantContext.Object, userContext.Object);

        var request = new AddTenantRequest
        {
            Name = "Tenant",
            SubDomain = "Tenant"
        };

        var result = await service.AddTenant(request);

        Assert.AreEqual("tenant", result.SubDomain);

        Assert.ContainsSingle(db.Tenants);
        Assert.ContainsSingle(db.UserTenants);
    }
    
    [Fact]
    public async Task AddTenant_Throws_WhenUserNotFound()
    {
        var db = CreateDb();

        var tenantContext = new Mock<ITenantContext>();
        var userContext = new Mock<ICmsUserContext>();

        userContext.Setup(u => u.UserId).Returns("missing");

        var service = new TenantsService(db, tenantContext.Object, userContext.Object);

        var request = new AddTenantRequest
        {
            Name = "Tenant",
            SubDomain = "tenant"
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.AddTenant(request));
    }
    
    [Fact]
    public async Task GetTenant_ReturnsTenantFromContext()
    {
        var tenant = new Tenant
        {
            Id = 1,
            Name = "Tenant",
            SubDomain = "tenant",
            RowVersion = [1]
        };

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.Setup(t => t.Tenant).Returns(tenant);

        var db = CreateDb();
        var userContext = new Mock<ICmsUserContext>();

        var service = new TenantsService(db, tenantContext.Object, userContext.Object);

        var result = await service.GetTenant();

        Assert.AreEqual("Tenant", result.Name);
    }
    
    [Fact]
    public async Task UpdateTenant_UpdatesTenant()
    {
        var db = CreateDb();

        var tenant = new Tenant
        {
            Id = 1,
            Name = "Old",
            SubDomain = "old",
            RowVersion = [1]
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var membership = new UserTenant
        {
            Role = UserTenantRole.Admin
        };

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.Setup(t => t.Tenant).Returns(tenant);
        tenantContext.Setup(t => t.Membership).Returns(membership);

        var userContext = new Mock<ICmsUserContext>();

        var service = new TenantsService(db, tenantContext.Object, userContext.Object);

        var request = new UpdateTenantRequest
        {
            Name = "New",
            SubDomain = "new",
            RowVersion = tenant.RowVersion
        };

        var result = await service.UpdateTenant(request);

        Assert.AreEqual("New", result.Name);
    }
    
    [Fact]
    public async Task UpdateTenant_Throws_WhenNotAdmin()
    {
        var db = CreateDb();

        var tenant = new Tenant
        {
            Id = 1,
            Name = "Tenant",
            SubDomain = "tenant"
        };

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.Setup(t => t.Tenant).Returns(tenant);
        tenantContext.Setup(t => t.Membership).Returns(new UserTenant
        {
            Role = UserTenantRole.Viewer
        });

        var userContext = new Mock<ICmsUserContext>();

        var service = new TenantsService(db, tenantContext.Object, userContext.Object);

        var request = new UpdateTenantRequest
        {
            Name = "Updated",
            SubDomain = "updated"
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.UpdateTenant(request));
    }
    
    [Fact]
    public async Task UpdateTenant_ShouldThrow_WhenUserNotInTenant()
    {
        var db = CreateDb();

        var tenant = new Tenant
        {
            Id = 1,
            Name = "TenantA",
            SubDomain = "a",
            RowVersion = [1]
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.Setup(t => t.Tenant).Returns((Tenant?)null);

        var userContext = new Mock<ICmsUserContext>();

        var service = new TenantsService(db, tenantContext.Object, userContext.Object);

        var request = new UpdateTenantRequest
        {
            Name = "Hacked",
            SubDomain = "hacked",
            RowVersion = [1]
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.UpdateTenant(request));
    }
    
    [Fact]
    public async Task UpdateTenant_ShouldThrow_WhenUserIsNotAdmin()
    {
        var db = CreateDb();

        var tenant = new Tenant
        {
            Id = 1,
            Name = "Tenant",
            SubDomain = "tenant",
            RowVersion = new byte[] {1}
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var tenantContext = new Mock<ITenantContext>();

        tenantContext.Setup(t => t.Tenant).Returns(tenant);
        tenantContext.Setup(t => t.Membership).Returns(new UserTenant
        {
            Role = UserTenantRole.Viewer
        });

        var userContext = new Mock<ICmsUserContext>();

        var service = new TenantsService(db, tenantContext.Object, userContext.Object);

        var request = new UpdateTenantRequest
        {
            Name = "NewName",
            SubDomain = "new",
            RowVersion = tenant.RowVersion
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.UpdateTenant(request));
    }
    
    [Fact]
    public async Task UpdateTenant_ShouldThrow_WhenSubdomainAlreadyExists()
    {
        var db = CreateDb();

        var tenantA = new Tenant
        {
            Id = 1,
            Name = "TenantA",
            SubDomain = "a",
            RowVersion = new byte[] {1}
        };

        var tenantB = new Tenant
        {
            Id = 2,
            Name = "TenantB",
            SubDomain = "b",
            RowVersion = new byte[] {1}
        };

        db.Tenants.AddRange(tenantA, tenantB);
        await db.SaveChangesAsync();

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.Setup(t => t.Tenant).Returns(tenantA);
        tenantContext.Setup(t => t.Membership).Returns(new UserTenant
        {
            Role = UserTenantRole.Admin
        });

        var userContext = new Mock<ICmsUserContext>();

        var service = new TenantsService(db, tenantContext.Object, userContext.Object);

        var request = new UpdateTenantRequest
        {
            Name = "TenantA",
            SubDomain = "b",
            RowVersion = tenantA.RowVersion
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.UpdateTenant(request));
    }
    
    [Fact]
    public async Task UpdateTenant_ShouldThrow_WhenRowVersionMismatch()
    {
        var db = CreateDb();

        var tenant = new Tenant
        {
            Id = 1,
            Name = "Tenant",
            SubDomain = "tenant",
            RowVersion = [1]
        };

        db.Tenants.Add(tenant);
        await db.SaveChangesAsync();

        var tenantContext = new Mock<ITenantContext>();
        tenantContext.Setup(t => t.Tenant).Returns(tenant);
        tenantContext.Setup(t => t.Membership).Returns(new UserTenant
        {
            Role = UserTenantRole.Admin
        });

        var userContext = new Mock<ICmsUserContext>();

        var service = new TenantsService(db, tenantContext.Object, userContext.Object);

        var request = new UpdateTenantRequest
        {
            Name = "Updated",
            SubDomain = "updated",
            RowVersion = new byte[] {9}
        };
        await Assert.ThrowsAsync<ConcurrencyException>(() =>
            service.UpdateTenant(request));
    }
    
    [Fact]
    public async Task AddTenant_ShouldThrow_WhenUserDoesNotExist()
    {
        var db = CreateDb();

        var tenantContext = new Mock<ITenantContext>();

        var userContext = new Mock<ICmsUserContext>();
        userContext.Setup(x => x.UserId).Returns("missing");

        var service = new TenantsService(db, tenantContext.Object, userContext.Object);

        var request = new AddTenantRequest
        {
            Name = "Tenant",
            SubDomain = "tenant"
        };

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            service.AddTenant(request));
    }
}
