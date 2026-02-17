namespace AgileSouthwestCMSAPI.Domain.DTOs;

public class TokenResult
{
    public string AccessToken { get; set; }
    public string IdToken { get; set; }
    public string RefreshToken { get; set; }
}