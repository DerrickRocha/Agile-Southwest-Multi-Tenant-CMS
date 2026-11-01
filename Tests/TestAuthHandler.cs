using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tests;

public class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers.TryGetValue("X-Test-User-Id", out var userIdHeader)
            ? userIdHeader.ToString()
            : "test-user";

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new("sub", userId)
        };

        if (Request.Headers.TryGetValue("X-Test-Role", out var roleHeader) &&
            !string.IsNullOrWhiteSpace(roleHeader))
        {
            claims.Add(new Claim(ClaimTypes.Role, roleHeader.ToString()));
        }

        var identity = new ClaimsIdentity(claims, "Test");

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}