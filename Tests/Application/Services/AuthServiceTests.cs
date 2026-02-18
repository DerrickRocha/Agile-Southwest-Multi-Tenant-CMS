using AgileSouthwestCMSAPI.Application.DTOs.Auth;
using AgileSouthwestCMSAPI.Application.Services;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Domain.Enums;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;
using Assert = Xunit.Assert;

namespace Tests.Application.Services;

public class AuthServiceTests
{
    private readonly Mock<ICognitoService> _cognitoMock = new();

    private AuthService CreateService(CmsDbContext db) => new(db, _cognitoMock.Object, true);

    private static CmsDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<CmsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new CmsDbContext(options);
    }

    // ----------------------------------------
    // SignupAsync - Happy Path
    // ----------------------------------------

    [Fact]
    public async Task SignupAsync_Should_Create_Tenant_And_User()
    {
        var db = CreateDbContext();

        var request = new SignupRequest
        {
            Email = "admin@test.com",
            Password = "Password123!",
            CompanyName = "Test Company",
            SubDomain = "Test Co"
        };

        _cognitoMock
            .Setup(x => x.SignUpAsync(request.Email, request.Password, "test-co"))
            .ReturnsAsync(new CognitoSignupResult
            {
                CognitoSub = "abc-123",
                UserConfirmed = false
            });

        var service = CreateService(db);

        var result = await service.SignupAsync(request);

        db.Tenants.Should().HaveCount(1);
        db.CmsUsers.Should().HaveCount(1);

        result.CognitoSub.Should().Be("abc-123");
        result.UserConfirmed.Should().BeFalse();
    }

    // ----------------------------------------
    // SignupAsync - Duplicate Subdomain
    // ----------------------------------------

    [Fact]
    public async Task SignupAsync_Should_Throw_When_Subdomain_Exists()
    {
        var db = CreateDbContext();

        db.Tenants.Add(new Tenant
        {
            TenantId = Guid.NewGuid(),
            Name = "Existing",
            SubDomain = "test-co",
            Status = TenantStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var service = CreateService(db);

        var request = new SignupRequest
        {
            Email = "admin@test.com",
            Password = "Password123!",
            CompanyName = "Test Company",
            SubDomain = "Test Co"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.SignupAsync(request));
    }

    // ----------------------------------------
    // AuthenticateAsync - User Not Found
    // ----------------------------------------

    [Fact]
    public async Task AuthenticateAsync_Should_Throw_When_User_Not_Found()
    {
        var db = CreateDbContext();

        _cognitoMock
            .Setup(x => x.AuthenticateAsync("test@test.com", "pass"))
            .ReturnsAsync(new TokenResult());

        var service = CreateService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AuthenticateAsync("test@test.com", "pass"));
    }

    // ----------------------------------------
    // AuthenticateAsync - Invited User
    // ----------------------------------------

    [Fact]
    public async Task AuthenticateAsync_Should_Throw_When_User_Invited()
    {
        var db = CreateDbContext();

        var tenant = new Tenant
        {
            TenantId = Guid.NewGuid(),
            Name = "Test",
            SubDomain = "test",
            Status = TenantStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Tenants.Add(tenant);

        db.CmsUsers.Add(new CmsUser
        {
            CmsUserId = Guid.NewGuid(),
            CognitoUserId = "123445555",
            TenantId = tenant.TenantId,
            Email = "test@test.com",
            Role = UserRole.Admin,
            Status = UserStatus.Invited,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        _cognitoMock
            .Setup(x => x.AuthenticateAsync("test@test.com", "pass"))
            .ReturnsAsync(new TokenResult());

        var service = CreateService(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            service.AuthenticateAsync("test@test.com", "pass"));
    }

    // ----------------------------------------
    // AuthenticateAsync - Success
    // ----------------------------------------

    [Fact]
    public async Task AuthenticateAsync_Should_Return_Tokens_When_Valid()
    {
        var db = CreateDbContext();

        var tenant = new Tenant
        {
            TenantId = Guid.NewGuid(),
            Name = "Test",
            SubDomain = "test",
            Status = TenantStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Tenants.Add(tenant);

        db.CmsUsers.Add(new CmsUser
        {
            CmsUserId = Guid.NewGuid(),
            CognitoUserId = "1234567",
            TenantId = tenant.TenantId,
            Email = "test@test.com",
            Role = UserRole.Admin,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync();

        var expectedTokens = new TokenResult
        {
            AccessToken = "access"
        };

        _cognitoMock
            .Setup(x => x.AuthenticateAsync("test@test.com", "pass"))
            .ReturnsAsync(expectedTokens);

        var service = CreateService(db);

        var result = await service.AuthenticateAsync("test@test.com", "pass");

        result.Should().Be(expectedTokens);
    }
}