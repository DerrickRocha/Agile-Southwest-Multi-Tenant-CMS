namespace AgileSouthwestCMSAPI.Domain.DTOs;

public class ConfirmSignupRequest
{
    public string Email { get; set; } = null!;
    public string ConfirmationCode { get; set; } = null!;
}