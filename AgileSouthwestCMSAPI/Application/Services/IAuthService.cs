using AgileSouthwestCMSAPI.Application.DTOs.Auth;
using AgileSouthwestCMSAPI.Application.DTOs.Cognito;
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

        var cognitoResult = await cognito.SignUpAsync(
            request.Email,
            request.Password);

        var cognitoSub = cognitoResult.CognitoSub;
                
        await cognito.AdminAddUserToGroupAsync(request.Email, CognitoGroups.Admin);
        
        if (skipTransactionsForTesting)
        {
            // Simple path for tests
            return await WriteSignupData(request, normalizedSubdomain, cognitoSub, cognitoResult.UserConfirmed);
        }

        var strategy = database.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await database.Database.BeginTransactionAsync();
            try
            {
                var signupResult = await WriteSignupData(request, normalizedSubdomain, cognitoSub, cognitoResult.UserConfirmed);
                await transaction.CommitAsync();
                return signupResult;
            }
            catch
            {
                await transaction.RollbackAsync();
                if (!string.IsNullOrEmpty(cognitoSub))
                    await cognito.DeleteUserAsync(cognitoSub);
                throw;
            }
        });
    }

    private async Task<SignupResult> WriteSignupData(SignupRequest request, string normalizedSubdomain, string cognitoSub, bool userConfirmed)
    {
        var tenant = new Tenant
        {
            Name = request.CompanyName,
            SubDomain = normalizedSubdomain
        };
        
        var userTenant = new UserTenant { Role = UserTenantRole.Admin, Tenant = tenant};
        var user = new CmsUser
        {
            CognitoUserId = cognitoSub,
            Email = request.Email,
            Role = UserRole.Admin,
            Status = UserStatus.Active,
            UserTenants = [userTenant]
        };
                
        database.CmsUsers.Add(user);
        
        await database.SaveChangesAsync();
        return new SignupResult
        {
            TenantId = tenant.Id,
            UserId = user.Id,
            CognitoSub = cognitoSub,
            UserConfirmed = userConfirmed
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