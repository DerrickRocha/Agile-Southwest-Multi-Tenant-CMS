using AgileSouthwestCMSAPI.Api.Controllers;
using AgileSouthwestCMSAPI.Api.Requests.Products;
using AgileSouthwestCMSAPI.Application.DTOs.Products;
using AgileSouthwestCMSAPI.Application.Interfaces;
using AgileSouthwestCMSAPI.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Tests.Api.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductsService> _service = new();
    
    private ProductsController CreateController() => new(_service.Object);
    
    [Fact]
    public async Task Create_ReturnsOk_With_Product()
    {
        var request = new ProductRequest
        {
            Name = "Test Product",
            Description = "Test Description",
            BasePrice = 1000,
            IsActive = true,
            Options = []
        };

        var result = new ProductResult
        {
            Id = 42,
            Name = "Test Product",
            Description = "Test Description",
            BasePrice = 1000,
            IsActive = true
        };
        
        _service.Setup(s => s.CreateProduct(request)).ReturnsAsync(result);
        var controller = CreateController();
        
        var response = await controller.Create(request);
        var createdResult = Assert.IsInstanceOfType<CreatedAtActionResult>(response);
        Assert.AreEqual(nameof(ProductsController.Get), createdResult.ActionName);
        Assert.AreEqual(result, createdResult.Value);
        

        _service.Verify(s => s.CreateProduct(request), Times.Once);
    }
}