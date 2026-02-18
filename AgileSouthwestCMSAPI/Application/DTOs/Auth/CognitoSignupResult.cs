namespace AgileSouthwestCMSAPI.Application.DTOs.Auth;

public class CognitoSignupResult
{
    public string CognitoSub { get; init; } = null!;
    public bool UserConfirmed { get; init; }
}