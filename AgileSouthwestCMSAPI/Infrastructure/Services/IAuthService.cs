using AgileSouthwestCMSAPI.Domain.DTOs;

namespace AgileSouthwestCMSAPI.Infrastructure.Services;

public interface IAuthService
{
    public Task<AuthResponse> SignupAsync(SignupRequest request);
}

public class AuthService(): IAuthService
{
    public Task<AuthResponse> SignupAsync(SignupRequest request)
    {
        throw new NotImplementedException();
    }
}