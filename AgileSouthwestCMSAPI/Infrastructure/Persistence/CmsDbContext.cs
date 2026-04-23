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

    public DbSet<Inventory> Inventory => Set<Inventory>();

    public DbSet<Store> Stores => Set<Store>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Tenant>(entity =>
        {
            entity.ToTable("tenants");

            entity.HasKey(t => t.Id);

            entity.Property(t => t.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

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

            entity.Property(t => t.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("DATETIME(6)");

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

            entity.HasKey(p => new { p.Id, p.TenantId });

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

            entity.Property(p => p.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("DATETIME(6)");

            entity.Property(p => p.RowVersion)
                .HasColumnName("row_version")
                .IsRowVersion();

            // Indexes
            entity.HasIndex(p => p.TenantId)
                .HasDatabaseName("product_tenant_idx");

            entity.HasIndex(p => new { p.TenantId, p.IsActive })
                .HasDatabaseName("product_tenant_active_idx");

            entity.HasIndex(p => new { p.TenantId, p.Name })
                .HasDatabaseName("product_tenant_name_idx");

            entity.HasQueryFilter(p => p.DeletedAt == null);
        });

        builder.Entity<ProductOption>(entity =>
        {
            entity.ToTable("product_options");
            entity.HasKey(p => new { p.Id, p.TenantId });

            entity.Property(o => o.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(o => o.TenantId)
                .HasColumnName("tenant_id")
                .IsRequired();

            entity.Property(p => p.ProductId)
                .HasColumnName("product_id")
                .IsRequired();
            entity.HasOne(o => o.Product)
                .WithMany(p => p.ProductOptions)
                .HasForeignKey(o => new { o.ProductId, o.TenantId })
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(o => o.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(o => o.IsRequired)
                .HasColumnName("is_required")
                .IsRequired();

            entity.Property(o => o.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.Property(o => o.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.Property(o => o.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("DATETIME(6)");

            entity.Property(o => o.RowVersion)
                .HasColumnName("row_version")
                .IsRowVersion();

            entity.HasIndex(o => new { o.ProductId, o.TenantId })
                .HasDatabaseName("product_option_product_idx");

            entity.HasIndex(o => o.TenantId)
                .HasDatabaseName("product_option_tenant_idx");

            entity.HasQueryFilter(o => o.DeletedAt == null);
        });

        builder.Entity<ProductOptionChoice>(entity =>
        {
            entity.ToTable("product_option_choices");
            entity.HasKey(p => new { p.Id, p.TenantId });

            entity.Property(o => o.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(c => c.TenantId)
                .HasColumnName("tenant_id")
                .IsRequired();

            entity.Property(p => p.ProductOptionId)
                .HasColumnName("option_id")
                .IsRequired();

            entity.HasOne(o => o.ProductOption)
                .WithMany(p => p.ProductOptionChoices)
                .HasForeignKey(c => new { c.ProductOptionId, c.TenantId })
                .HasPrincipalKey(po => new { po.Id, po.TenantId }) // Explicit principal key
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(o => o.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(o => o.PriceDeltaCents)
                .HasColumnName("price_delta_cents")
                .IsRequired();

            entity.Property(o => o.SalePriceDeltaCents)
                .HasColumnName("sale_price_delta_cents")
                .IsRequired();

            entity.Property(o => o.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.Property(o => o.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.Property(o => o.IsActive)
                .HasColumnName("is_active")
                .IsRequired();

            entity.Property(o => o.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.Property(t => t.RowVersion)
                .HasColumnName("row_version")
                .IsRowVersion();

            entity.HasIndex(c => new { c.ProductOptionId, c.TenantId })
                .HasDatabaseName("product_option_choice_option_idx");

            entity.HasIndex(c => c.TenantId)
                .HasDatabaseName("product_option_choice_tenant_idx");

            entity.HasQueryFilter(c => c.DeletedAt == null);
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
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.Property(u => u.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.Property(u => u.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("DATETIME(6)");

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

            entity.Property(ut => ut.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.Property(ut => ut.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");

            entity.Property(ut => ut.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("DATETIME(6)");

            entity.Property(t => t.RowVersion)
                .HasColumnName("row_version")
                .IsRowVersion();

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

        builder.Entity<Store>(entity =>
        {
            entity.ToTable("stores");

            entity.HasKey(s => new { s.Id, s.TenantId });
            entity.Property(s => s.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(s => s.TenantId)
                .HasColumnName("tenant_id")
                .IsRequired();


            entity.HasOne(s => s.Tenant)
                .WithMany(t => t.Stores)
                .HasForeignKey(s => s.TenantId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("stores_tenant_id_fk");

            entity.Property(s => s.Name)
                .HasColumnName("name")
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(s => s.SubDomain)
                .HasColumnName("sub_domain")
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(s => s.IsOnline)
                .HasColumnName("is_online")
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(s => s.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            entity.Property(s => s.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            entity.Property(s => s.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("DATETIME(6)");

            entity.Property(t => t.RowVersion)
                .HasColumnName("row_version")
                .IsRowVersion();

            entity.HasIndex(s => new { s.TenantId, s.SubDomain })
                .IsUnique()
                .HasDatabaseName("stores_tenant_subdomain_uk");

            entity.HasIndex(s => s.TenantId)
                .HasDatabaseName("stores_tenant_id_idx");

            entity.HasQueryFilter(s => s.DeletedAt == null);
        });

        builder.Entity<Inventory>(entity =>
            {
                entity.ToTable("inventory");
                entity.HasKey(i => new { i.Id, i.TenantId });

                entity.Property(i => i.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd();

                entity.Property(i => i.TenantId)
                    .HasColumnName("tenant_id")
                    .IsRequired();

                entity.Property(i => i.StoreId)
                    .HasColumnName("store_id")
                    .IsRequired();

                entity.Property(i => i.ProductId)
                    .HasColumnName("product_id")
                    .IsRequired();

                entity.Property(i => i.Quantity)
                    .HasColumnName("quantity")
                    .IsRequired()
                    .HasDefaultValue(0);

                entity.Property(i => i.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("DATETIME(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .IsRequired();

                entity.Property(i => i.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("DATETIME(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .IsRequired();

                entity.Property(i => i.DeletedAt)
                    .HasColumnName("deleted_at")
                    .HasColumnType("DATETIME(6)");

                entity.Property(i => i.RowVersion)
                    .HasColumnName("row_version")
                    .IsRowVersion();

                entity.HasOne(i => i.Tenant)
                    .WithMany(t => t.Inventory)
                    .HasForeignKey(s => s.TenantId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("inventory_tenant_id_fk");

                // Foreign key to Store (composite)
                entity.HasOne(i => i.Store)
                    .WithMany(s => s.Inventory)
                    .HasForeignKey(i => new { i.StoreId, i.TenantId })
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("inventory_store_tenant_id_fk");

                // Foreign key to Product (composite)
                entity.HasOne(i => i.Product)
                    .WithMany(p => p.Inventory)
                    .HasForeignKey(i => new { i.ProductId, i.TenantId })
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("inventory_product_tenant_id_fk");

                // Unique constraint
                entity.HasIndex(i => new { i.TenantId, i.StoreId, i.ProductId })
                    .IsUnique()
                    .HasDatabaseName("inventory_tenant_store_product_uk");

                // Indexes
                entity.HasIndex(i => i.TenantId)
                    .HasDatabaseName("inventory_tenant_id_idx");

                entity.HasIndex(i => new { i.TenantId, i.StoreId })
                    .HasDatabaseName("inventory_tenant_store_idx");

                entity.HasIndex(i => new { i.TenantId, i.ProductId })
                    .HasDatabaseName("inventory_tenant_product_idx");

                entity.HasIndex(i => new { i.TenantId, i.Quantity })
                    .HasDatabaseName("inventory_tenant_quantity_idx");

                entity.HasIndex(i => new { i.TenantId, i.DeletedAt })
                    .HasDatabaseName("inventory_tenant_deleted_idx");

                // Check constraint for quantity >= 0
                entity.ToTable(t => t.HasCheckConstraint("CK_inventory_quantity", "quantity >= 0"));

                entity.HasQueryFilter(i => i.DeletedAt == null);
            }
        );

        builder.Entity<Image>(entity =>
            {
                entity.ToTable("images");

                entity.HasKey(i => new { i.Id, i.TenantId });

                entity.Property(i => i.Id)
                    .HasColumnName("id")
                    .IsRequired()
                    .ValueGeneratedOnAdd();
                
                entity.Property(i => i.TenantId)
                    .HasColumnName("tenant_id")
                    .IsRequired();

                entity.Property(i => i.Url)
                    .HasColumnName("url")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(i => i.OriginalFileName)
                    .HasColumnName("original_filename")
                    .HasMaxLength(255)
                    .IsRequired();

                entity.Property(i => i.FileSize)
                    .IsRequired();

                entity.Property(i => i.ContentType)
                    .HasMaxLength(100)
                    .IsRequired();
                
                entity.Property(i => i.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("DATETIME(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .IsRequired();

                entity.Property(i => i.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("DATETIME(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .IsRequired();

                entity.Property(i => i.DeletedAt)
                    .HasColumnName("deleted_at")
                    .HasColumnType("DATETIME(6)");
            }
            
        );
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
        var entries = ChangeTracker.Entries();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Property("CreatedAt") != null)
                    entry.Property("CreatedAt").CurrentValue = now;
                if (entry.Property("UpdatedAt") != null)
                    entry.Property("UpdatedAt").CurrentValue = now;
            }
            else if (entry.State == EntityState.Modified)
            {
                if (entry.Property("UpdatedAt") != null)
                    entry.Property("UpdatedAt").CurrentValue = now;
            }
        }
    }
}