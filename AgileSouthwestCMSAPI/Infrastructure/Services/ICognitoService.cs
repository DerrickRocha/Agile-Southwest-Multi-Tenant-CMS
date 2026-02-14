using System.Security.Cryptography;
using System.Text;
using AgileSouthwestCMSAPI.Domain.DTOs;
using AgileSouthwestCMSAPI.Infrastructure.Exceptions;
using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;

namespace AgileSouthwestCMSAPI.Infrastructure.Services;

public interface ICognitoService
{
    Task<CognitoSignupResult> SignUpAsync(string email, string password, string tenantIdentifier);
    Task<TokenResult> AuthenticateAsync(string email, string password);
    Task DeleteUserBySubAsync(string sub);
}

public class CognitoService(
    IOptions<CognitoSettings> settingsOptions
)
    : ICognitoService
{
    private readonly CognitoSettings _settings = settingsOptions.Value;
    private readonly IAmazonCognitoIdentityProvider _provider = new AmazonCognitoIdentityProviderClient(
        RegionEndpoint.GetBySystemName(settingsOptions.Value.Region)
    );
    

    public async Task<CognitoSignupResult> SignUpAsync(string email, string password, string tenantIdentifier)
    {
        try
        {
            var request = new SignUpRequest
            {
                ClientId = _settings.ClientId,
                Username = email,
                Password = password,
                SecretHash = CalculateSecretHash(email),
                UserAttributes = new List<AttributeType>
                {
                    new() { Name = "email", Value = email },
                    new() { Name = "custom:tenant", Value = tenantIdentifier }
                }
            };

            var response = await _provider.SignUpAsync(request);

            return new CognitoSignupResult
            {
                CognitoSub = response.UserSub,
                UserConfirmed = response.UserConfirmed ?? false
            };
        }
        catch (UsernameExistsException)
        {
            throw new CognitoValidationException("User already exists.");
        }
        catch (InvalidPasswordException ex)
        {
            throw new CognitoValidationException(ex.Message);
        }
        catch (InvalidParameterException ex)
        {
            throw new CognitoValidationException(ex.Message);
        }
    }

    public Task<TokenResult> AuthenticateAsync(string email, string password)
    {
        throw new NotImplementedException();
    }

    public Task DeleteUserBySubAsync(string sub)
    {
        throw new NotImplementedException();
    }

    private string CalculateSecretHash(string username)
    {
        if (string.IsNullOrEmpty(_settings.ClientSecret))
            return string.Empty;

        var message = username + _settings.ClientId;
        var keyBytes = Encoding.UTF8.GetBytes(_settings.ClientSecret);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(messageBytes);

        return Convert.ToBase64String(hash);
    }
}