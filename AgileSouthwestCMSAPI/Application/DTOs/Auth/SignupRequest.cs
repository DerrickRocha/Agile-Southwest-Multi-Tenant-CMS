namespace AgileSouthwestCMSAPI.Application.DTOs.Auth;

public class SignupRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string CompanyName { get; set; }
    public string SubDomain { get; set; }
}