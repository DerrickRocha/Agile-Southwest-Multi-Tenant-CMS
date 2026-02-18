namespace AgileSouthwestCMSAPI.Application.DTOs.Auth;

public class SignupResult
{
    public Guid TenantId { get; init; }

    public Guid UserId { get; init; }

    public string CognitoSub { get; init; } = null!;

    public bool UserConfirmed { get; init; }

    public bool RequiresEmailConfirmation => !UserConfirmed;
}