using AgileSouthwestCMSAPI.Api.Controllers;
using AgileSouthwestCMSAPI.Api.Requests.Products;
using AgileSouthwestCMSAPI.Application.DTOs.Products;
using AgileSouthwestCMSAPI.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.Api.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductsService> _service = new();
    
    private ProductsController CreateController() => new(_service.Object);
    
    [Fact]
    public async Task GetAll_ReturnsOk_WithPagedResults()
    {
        var query = new GetProductsQuery(Page: 1, PageSize: 20);
        var result = new PagedResult<ProductResult>(
            [
                new ProductResult()
                {
                    Id = 1,
                    Name = "Product 1",
                    Description = "Description 1",
                    BasePrice = 1000,
                    IsActive = true
                },
                new ProductResult()
                {
                    Id = 2,
                    Name = "Product 2",
                    Description = "Description 2",
                    BasePrice = 2000,
                }
            ],
            1,
            20,
            2);

        _service.Setup(s => s.GetProducts(query)).ReturnsAsync(result);

        var controller = CreateController();

        var response = await controller.GetAll(query);

        var okResult = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(result, okResult.Value);

        _service.Verify(s => s.GetProducts(query), Times.Once);
    }

    [Fact]
    public async Task Patch_ReturnsOk_WithPatchedProduct()
    {
        var request = new PatchProductRequest(
            Name: "Updated Name",
            Description: "Updated Description",
            BasePrice: 1500,
            IsActive: true,
            Options: null);

        var result = new ProductResult
        {
            Id = 10,
            Name = "Updated Name",
            Description = "Updated Description",
            BasePrice = 1500,
            IsActive = true
        };

        _service.Setup(s => s.PatchProduct(10, request)).ReturnsAsync(result);

        var controller = CreateController();

        var response = await controller.Patch(10, request);

        var okResult = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(result, okResult.Value);

        _service.Verify(s => s.PatchProduct(10, request), Times.Once);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WithProduct()
    {
        var request = new ProductRequest
        {
            Name = "Coffee",
            Description = "Fresh coffee",
            BasePrice = 1000,
            IsActive = true,
            Options = []
        };

        var result = new ProductResult
        {
            Id = 42,
            Name = "Coffee",
            Description = "Fresh coffee",
            BasePrice = 1000,
            IsActive = true
        };

        _service.Setup(s => s.CreateProduct(request)).ReturnsAsync(result);

        var controller = CreateController();

        var response = await controller.Create(request);

        var createdResult = Assert.IsType<CreatedAtActionResult>(response);
        Assert.Equal(nameof(ProductsController.Get), createdResult.ActionName);
        Assert.Equal(result.Id, createdResult.RouteValues!["id"]);
        Assert.Equal(result, createdResult.Value);

        _service.Verify(s => s.CreateProduct(request), Times.Once);
    }

    [Fact]
    public async Task Get_ReturnsOk_WithProduct()
    {
        var result = new ProductResult
        {
            Id = 5,
            Name = "Coffee",
            Description = "Fresh coffee",
            BasePrice = 1000,
            IsActive = true
        };

        _service.Setup(s => s.GetProduct(5)).ReturnsAsync(result);

        var controller = CreateController();

        var response = await controller.Get(5);

        var okResult = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(result, okResult.Value);

        _service.Verify(s => s.GetProduct(5), Times.Once);
    }

    [Fact]
    public async Task Update_ReturnsOk_WithUpdatedProduct()
    {
        var request = new ProductRequest
        {
            Name = "Updated Coffee",
            Description = "Updated Description",
            BasePrice = 1200,
            IsActive = true,
            Options = []
        };

        var result = new ProductResult
        {
            Id = 5,
            Name = "Updated Coffee",
            Description = "Updated Description",
            BasePrice = 1200,
            IsActive = true
        };

        _service.Setup(s => s.UpdateProduct(5, request)).ReturnsAsync(result);

        var controller = CreateController();

        var response = await controller.Update(5, request);

        var okResult = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(result, okResult.Value);

        _service.Verify(s => s.UpdateProduct(5, request), Times.Once);
    }
}