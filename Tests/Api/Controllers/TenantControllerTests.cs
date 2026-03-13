using AgileSouthwestCMSAPI.Api.Controllers;
using AgileSouthwestCMSAPI.Application.DTOs.Tenants;
using AgileSouthwestCMSAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Tests.Api.Controllers;

public class TenantControllerTests
{
    private readonly Mock<ITenantsService> _tenantService = new();
    private TenantController CreateController() => new(_tenantService.Object);
    
    [Fact]
    public async Task GetTenant_ReturnsOk_WithTenant()
    {
        var result = new GetTenantResult
        {
            TenantId = 1,
            Name = "Test Tenant",
            SubDomain = "test",
            CustomDomain = "",
            RowVersion = [1]
        };

        _tenantService
            .Setup(s => s.GetTenant())
            .ReturnsAsync(result);

        var controller = CreateController();
        var response = await controller.GetTenant();

        var ok = Assert.IsInstanceOfType<OkObjectResult>(response);
        var value = Assert.IsInstanceOfType<GetTenantResult>(ok.Value);

        Assert.AreEqual("Test Tenant", value.Name);
    }

    [Fact]
    public async Task AddTenant_ReturnsOk_WithCreatedTenant()
    {
        var request = new AddTenantRequest
        {
            Name = "Tenant",
            SubDomain = "tenant"
        };

        var result = new AddTenantResult
        {
            TenantId = 1,
            Name = "Tenant",
            SubDomain = "tenant",
            CustomDomain = "",
            RowVersion = [1]
        };

        _tenantService
            .Setup(s => s.AddTenant(request))
            .ReturnsAsync(result);
    var controller = CreateController();
        var response = await controller.AddTenant(request);

        var ok = Assert.IsInstanceOfType<OkObjectResult>(response);
        var value = Assert.IsInstanceOfType<AddTenantResult>(ok.Value);

        Assert.AreEqual(1, value.TenantId);
    }

    [Fact]
    public async Task UpdateTenant_ReturnsOk_WithUpdatedTenant()
    {
        var request = new UpdateTenantRequest
        {
            Name = "Updated",
            SubDomain = "updated",
            RowVersion = [1]
        };

        var result = new UpdateTenantResult
        {
            Id = 1,
            Name = "Updated",
            SubDomain = "updated",
            CustomDomain = "",
            RowVersion = [2]
        };

        _tenantService
            .Setup(s => s.UpdateTenant(request))
            .ReturnsAsync(result);

        var controller = CreateController();
        var response = await controller.UpdateTenant(request);

        var ok = Assert.IsInstanceOfType<OkObjectResult>(response);
        var value = Assert.IsInstanceOfType<UpdateTenantResult>(ok.Value);

        Assert.AreEqual("Updated", value.Name);
    }
    
}