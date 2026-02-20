using AgileSouthwestCMSAPI.Application.DTOs.Auth;
using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Domain.Enums;
using AgileSouthwestCMSAPI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Application.Services;

public interface IAuthService
{
    public Task<SignupResult> SignupAsync(SignupRequest request);

    Task<TokenResult> AuthenticateAsync(string email, string password);
}

public class AuthService(
    CmsDbContext database,
    ICognitoService cognito,
    bool skipTransactionsForTesting = false
) : IAuthService
{
    public async Task<SignupResult> SignupAsync(SignupRequest request)
    {
        var normalizedSubdomain = Normalize(request.SubDomain);

        if (await database.Tenants.AnyAsync(t => t.SubDomain == normalizedSubdomain))
            throw new InvalidOperationException("Subdomain already taken.");

        string? cognitoSub = null;

        if (skipTransactionsForTesting)
        {
            // Simple path for tests
            return await SignupInternal(request, normalizedSubdomain);
        }

        var strategy = database.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await database.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ Create Cognito user
                var cognitoResult = await cognito.SignUpAsync(
                    request.Email,
                    request.Password);

                cognitoSub = cognitoResult.CognitoSub;

                // 2️⃣ Create Tenant
                var guid = Guid.NewGuid();
                var tenant = new Tenant
                {
                    TenantId = guid,
                    Name = request.CompanyName,
                    SubDomain = normalizedSubdomain,
                    Status = TenantStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                database.Tenants.Add(tenant);

                // 3️⃣ Create User
                var user = new CmsUser
                {
                    CmsUserId = Guid.NewGuid(),
                    CognitoUserId = cognitoSub,
                    Email = request.Email,
                    Role = UserRole.Admin,
                    Status = UserStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                database.CmsUsers.Add(user);

                await database.SaveChangesAsync();
                await transaction.CommitAsync();

                return new SignupResult
                {
                    TenantId = tenant.TenantId,
                    UserId = user.CmsUserId,
                    CognitoSub = cognitoSub,
                    UserConfirmed = cognitoResult.UserConfirmed
                };
            }
            catch
            {
                await transaction.RollbackAsync();

                if (!string.IsNullOrEmpty(cognitoSub))
                    await cognito.DeleteUserBySubAsync(cognitoSub);

                throw;
            }
        });
    }

    private async Task<SignupResult> SignupInternal(SignupRequest request, string normalizedSubdomain)
    {
        // 1️⃣ Create Cognito user
        var cognitoResult = await cognito.SignUpAsync(
            request.Email,
            request.Password);

        var cognitoSub = cognitoResult.CognitoSub;

        // 2️⃣ Create Tenant
        var tenant = new Tenant
        {
            TenantId = Guid.NewGuid(),
            Name = request.CompanyName,
            SubDomain = normalizedSubdomain,
            Status = TenantStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        database.Tenants.Add(tenant);

        // 3️⃣ Create User
        var user = new CmsUser
        {
            CmsUserId = Guid.NewGuid(),
            CognitoUserId = cognitoSub,
            Email = request.Email,
            Role = UserRole.Admin,
            Status = UserStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        database.CmsUsers.Add(user);

        await database.SaveChangesAsync();

        return new SignupResult
        {
            TenantId = tenant.TenantId,
            UserId = user.CmsUserId,
            CognitoSub = cognitoSub,
            UserConfirmed = cognitoResult.UserConfirmed
        };
    }

    public async Task<TokenResult> AuthenticateAsync(string email, string password)
    {
        var tokens = await cognito.AuthenticateAsync(email, password);
        var user = await database.CmsUsers
            .FirstOrDefaultAsync(u => u.Email == email);

        if (user == null)
            throw new InvalidOperationException("User not found.");

        if (user.Status != UserStatus.Active)
            throw new InvalidOperationException("User is inactive.");

        return tokens;
    }

    private string Normalize(string input)
    {
        return input
            .Trim()
            .ToLower()
            .Replace(" ", "-");
    }
}