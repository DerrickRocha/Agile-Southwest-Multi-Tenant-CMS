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
    Task ConfirmSignUpAsync(string email, string confirmationCode);
    Task ResendConfirmationCodeAsync(string email);
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

    public async Task<TokenResult> AuthenticateAsync(string email, string password)
    {
        try
        {
            var request = new InitiateAuthRequest
            {
                ClientId = _settings.ClientId,
                AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
                AuthParameters = new Dictionary<string, string>
                {
                    ["USERNAME"] = email,
                    ["PASSWORD"] = password,
                }
            };

            var response = await provider.InitiateAuthAsync(request);

            var auth = response.AuthenticationResult;
            if (auth == null)
                throw new CognitoValidationException("Authentication failed: no tokens returned.");

            return new TokenResult
            {
                AccessToken = auth.AccessToken ?? string.Empty,
                IdToken = auth.IdToken ?? string.Empty,
                RefreshToken = auth.RefreshToken ?? string.Empty
            };
        }
        catch (NotAuthorizedException)
        {
            throw new CognitoValidationException("Invalid email or password.");
        }
        catch (UserNotConfirmedException)
        {
            throw new CognitoValidationException("USER_NOT_CONFIRMED");
        }
        catch (UserNotFoundException)
        {
            throw new CognitoValidationException("User not found.");
        }
        catch (PasswordResetRequiredException)
        {
            throw new CognitoValidationException("Password reset is required.");
        }
        catch (InvalidParameterException ex)
        {
            throw new CognitoValidationException(ex.Message);
        }
        catch (InvalidLambdaResponseException ex)
        {
            throw new CognitoValidationException(ex.Message);
        }
        catch (TooManyRequestsException)
        {
            throw new CognitoValidationException("Too many requests. Please try again later.");
        }
    }

    public async Task ConfirmSignUpAsync(string email, string confirmationCode)
    {
        try
        {
            var request = new ConfirmSignUpRequest
            {
                ClientId = _settings.ClientId,
                Username = email,
                ConfirmationCode = confirmationCode,
            };

            await provider.ConfirmSignUpAsync(request);
        }
        catch (CodeMismatchException)
        {
            throw new CognitoValidationException("Invalid confirmation code.");
        }
        catch (ExpiredCodeException)
        {
            throw new CognitoValidationException("Confirmation code expired.");
        }
        catch (UserNotFoundException)
        {
            throw new CognitoValidationException("User not found.");
        }
        catch (NotAuthorizedException)
        {
            throw new CognitoValidationException("User is already confirmed.");
        }
        catch (InvalidParameterException ex)
        {
            throw new CognitoValidationException(ex.Message);
        }
    }

    public async Task ResendConfirmationCodeAsync(string email)
    {
        try
        {
            var request = new ResendConfirmationCodeRequest
            {
                ClientId = _settings.ClientId,
                Username = email,
            };

            await provider.ResendConfirmationCodeAsync(request);
        }
        catch (UserNotFoundException)
        {
            throw new CognitoValidationException("User not found.");
        }
        catch (InvalidParameterException ex)
        {
            throw new CognitoValidationException(ex.Message);
        }
        catch (TooManyRequestsException)
        {
            throw new CognitoValidationException("Too many requests. Please try again later.");
        }    }

    public async Task DeleteUserBySubAsync(string sub)
    {
        try
        {
            var request = new AdminDeleteUserRequest
            {
                UserPoolId = _settings.UserPoolId,
                Username = sub
            };

            await provider.AdminDeleteUserAsync(request);
        }
        catch
        {
            // We intentionally swallow errors here
            // because this is compensation logic
        }
    }
}