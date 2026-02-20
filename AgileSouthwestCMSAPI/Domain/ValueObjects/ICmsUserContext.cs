using System.Security.Claims;

namespace AgileSouthwestCMSAPI.Domain.ValueObjects;

public interface ICmsUserContext
{
    string UserId { get; }
}

public class CmsUserContext : ICmsUserContext
{
    public string UserId { get; }

    public CmsUserContext(IHttpContextAccessor accessor)
    {
        var context = accessor.HttpContext ?? throw new UnauthorizedAccessException("No active HttpContext");
        var user = context.User ?? throw new UnauthorizedAccessException("No user principal found");
        UserId = (user.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? user.FindFirst("sub")?.Value) ?? throw new UnauthorizedAccessException("User not found");
    }
}