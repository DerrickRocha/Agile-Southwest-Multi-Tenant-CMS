using AgileSouthwestCMSAPI.Api.Controllers;
using AgileSouthwestCMSAPI.Application.DTOs.Auth;
using AgileSouthwestCMSAPI.Application.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace Tests.Api.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IAuthService> _authServiceMock = new();
    private readonly Mock<ICognitoService> _cognitoMock = new();
    
    private AuthController CreateController() => new(_authServiceMock.Object, _cognitoMock.Object);

     // -------------------------------
    // Register
    // -------------------------------

    [Fact]
    public async Task Register_ReturnsOk_WithResult()
    {
        // Arrange
        var request = new SignupRequest { Email = "test@test.com", Password = "Password123!" };
        var expected = new SignupResult
        {
            TenantId = Guid.NewGuid(),
            CognitoSub = "123444",
            UserConfirmed = false,
            UserId = Guid.NewGuid(),
        };

        _authServiceMock
            .Setup(x => x.SignupAsync(request))
            .ReturnsAsync(expected);

        var controller = CreateController();

        // Act
        var result = await controller.Register(request);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(expected);
    }

    // -------------------------------
    // Login
    // -------------------------------

    [Fact]
    public async Task Login_ReturnsOk_WithTokens()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@test.com",
            Password = "Password123!"
        };

        var tokenResult = new TokenResult
        {
            AccessToken = "access",
            IdToken = "id",
            RefreshToken = "refresh"
        };

        _authServiceMock
            .Setup(x => x.AuthenticateAsync(request.Email, request.Password))
            .ReturnsAsync(tokenResult);

        var controller = CreateController();

        // Act
        var result = await controller.Login(request);

        // Assert
        var okResult = result as OkObjectResult;
        okResult.Should().NotBeNull();
        okResult!.Value.Should().Be(tokenResult);
    }

    // -------------------------------
    // Confirm
    // -------------------------------

    [Fact]
    public async Task Confirm_ReturnsOk()
    {
        // Arrange
        var request = new ConfirmSignupRequest
        {
            Email = "test@test.com",
            ConfirmationCode = "123456"
        };

        var controller = CreateController();

        // Act
        var result = await controller.Confirm(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _cognitoMock.Verify(
            x => x.ConfirmSignUpAsync(request.Email, request.ConfirmationCode),
            Times.Once);
    }

    // -------------------------------
    // Resend Confirmation
    // -------------------------------

    [Fact]
    public async Task ResendConfirmation_ReturnsOk()
    {
        // Arrange
        var request = new ResendConfirmationRequest
        {
            Email = "test@test.com"
        };

        var controller = CreateController();

        // Act
        var result = await controller.ResendConfirmation(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        _cognitoMock.Verify(
            x => x.ResendConfirmationCodeAsync(request.Email),
            Times.Once);
    }
}