namespace AgileSouthwestCMSAPI.Application.DTOs.Auth;

public class TokenResult
{
    public string AccessToken { get; set; }
    public string IdToken { get; set; }
    public string RefreshToken { get; set; }
}