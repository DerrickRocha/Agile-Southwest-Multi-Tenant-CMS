using System.Security.Cryptography;
using System.Text;
using AgileSouthwestCMSAPI.Domain.DTOs;
using AgileSouthwestCMSAPI.Infrastructure.Exceptions;
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
    IAmazonCognitoIdentityProvider provider,
    IOptions<CognitoSettings> settingsOptions
) : ICognitoService
{

    private readonly CognitoSettings _settings = settingsOptions.Value;
    
    public async Task<CognitoSignupResult> SignUpAsync(string email, string password, string tenantIdentifier)
    {
        try
        {
            var request = new SignUpRequest
            {
                ClientId = _settings.ClientId,
                Username = email,
                Password = password,
                UserAttributes =
                [
                    new AttributeType { Name = "email", Value = email },
                    new AttributeType { Name = "custom:tenant_id", Value = tenantIdentifier }
                ]
            };

            var response = await provider.SignUpAsync(request);

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
}