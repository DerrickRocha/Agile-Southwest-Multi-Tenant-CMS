namespace AgileSouthwestCMSAPI.Domain.DTOs;

public class AuthResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }

    public string AccessToken { get; set; }
    public string IdToken { get; set; }
    public string RefreshToken { get; set; }

    public static AuthResponse Conflict(string message) =>
        new AuthResponse { StatusCode = 409, Message = message };

    public static AuthResponse BadRequest(string message) =>
        new AuthResponse { StatusCode = 400, Message = message };

    public static AuthResponse Success(TokenResult tokens) =>
        new AuthResponse
        {
            StatusCode = 200,
            AccessToken = tokens.AccessToken,
            IdToken = tokens.IdToken,
            RefreshToken = tokens.RefreshToken
        };
}
