using AgileSouthwestCMSAPI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgileSouthwestCMSAPI.Infrastructure.Persistence;

public class CmsDbContext(DbContextOptions<CmsDbContext> options) : DbContext(options)
{
     public DbSet<User> Users => Set<User>();
    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // =========================
        // Tenant Configuration
        // =========================
        builder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.Id);

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
        builder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            entity.Property(u => u.CognitoSub)
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
            entity.HasIndex(u => u.CognitoSub)
                .IsUnique();

            entity.HasIndex(u => new { u.TenantId, u.Email })
                .IsUnique();
        });
    }
}