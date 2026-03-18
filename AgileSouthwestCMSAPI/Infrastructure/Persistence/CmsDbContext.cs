using AgileSouthwestCMSAPI.Domain.Entities;

namespace AgileSouthwestCMSAPI.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

public class CmsDbContext(DbContextOptions<CmsDbContext> options) : DbContext(options)
{
    public DbSet<CmsUser> CmsUsers => Set<CmsUser>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<UserTenant> UserTenants => Set<UserTenant>();

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductOption> ProductOptions => Set<ProductOption>();
    public DbSet<ProductOptionChoice> ProductOptionChoices => Set<ProductOptionChoice>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // =========================
        // tenants
        // =========================
        builder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");

            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd(); // AUTO_INCREMENT
            
            entity.Property(t => t.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(t => t.SubDomain)
                .HasColumnName("sub_domain")
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(t => t.CustomDomain)
                .HasColumnName("custom_domain")
                .HasMaxLength(255);

            entity.Property(t => t.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            entity.Property(t => t.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            entity.Property(t => t.RowVersion)
                .HasColumnName("row_version")
                .IsRowVersion();

            entity.HasIndex(t => t.SubDomain)
                .HasDatabaseName("uq_tenants_subdomain")
                .IsUnique();

            // MySQL unique indexes allow multiple NULL values, so no filter needed.
            entity.HasIndex(t => t.CustomDomain)
                .HasDatabaseName("uq_tenants_custom_domain")
                .IsUnique();
        });

        builder.Entity<Product>(entity =>
        {
            entity.ToTable("products");
            
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            
            entity.Property(p => p.TenantId)
                .HasColumnName("tenant_id")
                .IsRequired();
            entity.HasOne(p => p.Tenant)
                .WithMany(t => t.Products)
                .HasForeignKey(p => p.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(p => p.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(p => p.Description)
                .HasColumnName("description");
            
            entity.Property(p => p.BasePriceCents)
                .HasColumnName("base_price_cents")
                .IsRequired();

            entity.Property(p => p.IsActive)
                .HasColumnName("is_active")
                .IsRequired();
            
            entity.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            entity.Property(u => u.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();
        });

        builder.Entity<ProductOption>(entity =>
        {
            entity.ToTable("product_options");
            entity.HasKey(p => p.Id);
            entity.Property(o => o.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            
            entity.Property(p => p.ProductId)
                .HasColumnName("product_id")
                .IsRequired();
            entity.HasOne(o => o.Product)
                .WithMany(p => p.ProductOptions)
                .HasForeignKey(o => o.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Property(o => o.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(o => o.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            
            entity.Property(o => o.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
        });
        
        builder.Entity<ProductOptionChoice>(entity =>
        {
            entity.ToTable("product_option_choices");
            entity.HasKey(p => p.Id);
            entity.Property(o => o.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();
            
            entity.Property(p => p.ProductOptionId)
                .HasColumnName("option_id")
                .IsRequired();
            entity.HasOne(o => o.ProductOption)
                .WithMany(p => p.ProductOptionChoices)
                .HasForeignKey(o => o.ProductOptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // =========================
        // cms_users
        // =========================
        builder.Entity<CmsUser>(entity =>
        {
            entity.ToTable("cms_users");

            entity.HasKey(u => u.Id);

            entity.Property(u => u.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(u => u.CognitoUserId)
                .HasColumnName("cognito_user_id")
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(u => u.Email)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(u => u.Role)
                .HasColumnName("role")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(u => u.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            entity.Property(u => u.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            entity.HasIndex(u => u.Email)
                .HasDatabaseName("uq_cms_users_email")
                .IsUnique();

            // Optional but recommended if you rely on CognitoUserId uniqueness:
            entity.HasIndex(u => u.CognitoUserId)
                .HasDatabaseName("uq_cms_users_cognito_user_id")
                .IsUnique();
        });

        // =========================
        // user_tenants (join table)
        // =========================
        builder.Entity<UserTenant>(entity =>
        {
            entity.ToTable("user_tenants");

            // Your SQL PK order is (tenant_id, user_id)
            entity.HasKey(ut => new { ut.TenantId, ut.UserId });

            entity.Property(ut => ut.TenantId)
                .HasColumnName("tenant_id");

            entity.Property(ut => ut.UserId)
                .HasColumnName("user_id");

            entity.Property(ut => ut.Role)
                .HasColumnName("Role")
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            entity.HasOne(ut => ut.User)
                .WithMany(u => u.UserTenants)
                .HasForeignKey(ut => ut.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_tenant_user_fk");

            entity.HasOne(ut => ut.Tenant)
                .WithMany(t => t.UserTenants)
                .HasForeignKey(ut => ut.TenantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("user_tenant_tenant_fk");

            entity.HasIndex(ut => ut.UserId);
            entity.HasIndex(ut => ut.TenantId);
        });
    }

    // Optional: keep UpdatedAt accurate at app level (since your SQL default doesn’t auto-update it).
    public override int SaveChanges()
    {
        ApplyTimestamps();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void ApplyTimestamps()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Properties.Any(p => p.Metadata.Name == nameof(Tenant.CreatedAt)))
                    entry.Property(nameof(Tenant.CreatedAt)).CurrentValue = now;

                if (entry.Properties.Any(p => p.Metadata.Name == nameof(Tenant.UpdatedAt)))
                    entry.Property(nameof(Tenant.UpdatedAt)).CurrentValue = now;
            }

            if (entry.State == EntityState.Modified)
            {
                if (entry.Properties.Any(p => p.Metadata.Name == nameof(Tenant.UpdatedAt)))
                    entry.Property(nameof(Tenant.UpdatedAt)).CurrentValue = now;
            }
        }
    }
}