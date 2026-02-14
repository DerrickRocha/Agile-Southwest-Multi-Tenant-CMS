namespace AgileSouthwestCMSAPI.Domain.DTOs;

public class CognitoSettings
{
    public string UserPoolId { get; set; } = null!;
    public string ClientId { get; set; } = null!;
    public string ClientSecret { get; set; } = null!;
    public string Region { get; set; } = null!;
}
