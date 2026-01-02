using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Infrastructure.Persistence;

public class CmsDbContext(DbContextOptions<CmsDbContext> options) : DbContext(options)
{
}