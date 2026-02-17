using AgileSouthwestCMSAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AgileSouthwestCMSAPI.Infrastructure.Persistence;

public class CmsDbContext(DbContextOptions<CmsDbContext> options) : DbContext(options)
{
    public DbSet<CmsUser> CmsUsers => Set<CmsUser>();
    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        var guidConverter = new ValueConverter<Guid, byte[]>(
            guid => guid.ToByteArray(),
            bytes => new Guid(bytes));
        // =========================
        // Tenant Configuration
        // =========================
        builder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.TenantId);

            entity.Property(t => t.TenantId)
                .HasConversion(guidConverter)
                .HasColumnType("binary(16)")
                .ValueGeneratedNever();
            entity.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(t => t.SubDomain)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(t => t.CustomDomain)
                .HasMaxLength(255);

            entity.Property(t => t.PlanTier)
                .IsRequired();

            entity.Property(t => t.SubscriptionStatus)
                .IsRequired();

            entity.Property(t => t.Status)
                .IsRequired();

            entity.Property(t => t.CreatedAt)
                .IsRequired();

            entity.Property(t => t.UpdatedAt)
                .IsRequired();

            // Unique constraints
            entity.HasIndex(t => t.SubDomain)
                .IsUnique();

            entity.HasIndex(t => t.CustomDomain)
                .IsUnique()
                .HasFilter("[CustomDomain] IS NOT NULL");
        });

        // =========================
        // User Configuration
        // =========================
        builder.Entity<CmsUser>(entity =>
        {
            entity.HasKey(u => u.CmsUserId);
            
            entity.Property(u => u.CmsUserId)
                .HasConversion(guidConverter)
                .HasColumnType("binary(16)")
                .ValueGeneratedNever(); 


            entity.Property(u => u.CognitoUserId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(u => u.Role)
                .IsRequired();

            entity.Property(u => u.Status)
                .IsRequired();

            entity.Property(u => u.CreatedAt)
                .IsRequired();

            entity.Property(u => u.UpdatedAt)
                .IsRequired();

            // Foreign Key
            entity.HasOne(u => u.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TenantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Indexes
            entity.HasIndex(u => u.CognitoUserId)
                .IsUnique();

            entity.HasIndex(u => new { u.TenantId, u.Email })
                .IsUnique();
        });
    }
}