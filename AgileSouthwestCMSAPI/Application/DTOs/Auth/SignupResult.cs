namespace AgileSouthwestCMSAPI.Application.DTOs.Auth;

public class SignupResult
{
    public int TenantId { get; set; }

    public int UserId { get; set; }

    public string CognitoSub { get; init; } = null!;

    public bool UserConfirmed { get; init; }

    public bool RequiresEmailConfirmation => !UserConfirmed;
}