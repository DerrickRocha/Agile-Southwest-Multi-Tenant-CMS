using AgileSouthwestCMSAPI.Domain.Entities;
using AgileSouthwestCMSAPI.Domain.Enums;

namespace AgileSouthwestCMSAPI.Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

public class CmsDbContext(DbContextOptions<CmsDbContext> options) : DbContext(options)
{
    public DbSet<CmsUser> CmsUsers => Set<CmsUser>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<UserTenant> UserTenants => Set<UserTenant>();

    public DbSet<Image> Images => Set<Image>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductOption> ProductOptions => Set<ProductOption>();
    public DbSet<ProductOptionChoice> ProductOptionChoices => Set<ProductOptionChoice>();

    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    public DbSet<Inventory> Inventory => Set<Inventory>();

    public DbSet<Store> Stores => Set<Store>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<TaxCategory> TaxCategories => Set<TaxCategory>();

    public DbSet<ShippingZone> ShippingZones => Set<ShippingZone>();
    public DbSet<ZonePostalCode> ZonePostalCodes => Set<ZonePostalCode>();
    public DbSet<ShippingRate> ShippingRates => Set<ShippingRate>();

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

            entity.Property(c => c.RowVersion)
                .HasColumnName("row_version")
                .HasColumnType("TIMESTAMP")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                .IsRequired()
                .IsConcurrencyToken();

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

            entity.Property(p => p.TaxCategoryId)
                .HasColumnName("tax_category_id")
                .IsRequired();
            entity.HasOne(p => p.TaxCategory)
                .WithMany(tc => tc.Products)
                .HasForeignKey(p => p.TaxCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

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

        builder.Entity<ProductImage>(entity =>
        {
            entity.ToTable("product_images");

            entity.HasKey(pi => pi.Id);

            entity.Property(pi => pi.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd();

            entity.Property(pi => pi.TenantId)
                .HasColumnName("tenant_id")
                .IsRequired();

            entity.Property(pi => pi.ProductId)
                .HasColumnName("product_id")
                .IsRequired();

            entity.Property(pi => pi.ImageId)
                .HasColumnName("image_id")
                .IsRequired();

            entity.Property(pi => pi.Position)
                .HasColumnName("position")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(pi => pi.IsPrimary)
                .HasColumnName("is_primary")
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(pi => pi.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            entity.Property(pi => pi.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            entity.Property(pi => pi.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("DATETIME(6)")
                .IsRequired(false);

            // Relationships
            entity.HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => new { pi.TenantId, pi.ProductId })
                .HasPrincipalKey(p => new { p.TenantId, p.Id }) // Important!
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_product_images_product");

            entity.HasOne(pi => pi.Image)
                .WithMany(i => i.ProductImages) // Requires ICollection<ProductImage> on Image
                .HasForeignKey(pi => pi.ImageId)
                .HasPrincipalKey(i => i.Id) // Specify principal key
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_product_images_image");

            // Indexes
            entity.HasIndex(pi => new { pi.TenantId, pi.ProductId, pi.ImageId })
                .IsUnique()
                .HasDatabaseName("uk_active_product_image");

            entity.HasIndex(pi => new { pi.TenantId, pi.ProductId, pi.Position })
                .IsUnique()
                .HasDatabaseName("uk_position_per_product");

            entity.HasIndex(pi => new { pi.ProductId, pi.TenantId })
                .HasDatabaseName("image_product_idx");

            entity.HasIndex(pi => pi.TenantId)
                .HasDatabaseName("image_product_tenant_idx");

            entity.HasIndex(pi => new { pi.ProductId, pi.IsPrimary })
                .HasDatabaseName("image_primary_idx");

            entity.HasIndex(pi => pi.DeletedAt)
                .HasDatabaseName("idx_deleted");

            entity.HasQueryFilter(pi => pi.DeletedAt == null);
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

                entity.HasKey(i => i.Id);

                entity.Property(i => i.Id)
                    .HasColumnName("id")
                    .IsRequired()
                    .ValueGeneratedOnAdd();

                entity.Property(i => i.TenantId)
                    .HasColumnName("tenant_id")
                    .IsRequired();

                entity.Property(i => i.Url)
                    .HasColumnName("url")
                    .HasMaxLength(2048)
                    .IsRequired();

                entity.Property(i => i.OriginalFileName)
                    .HasColumnName("original_filename")
                    .HasMaxLength(255)
                    .IsRequired(false);

                entity.Property(i => i.FileSize)
                    .HasColumnName("file_size")
                    .IsRequired(false);

                entity.Property(i => i.ContentType)
                    .HasColumnName("content_type")
                    .HasMaxLength(100)
                    .IsRequired(false);

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
                
                entity.Property(c => c.RowVersion)
                    .HasColumnName("row_version")
                    .HasColumnType("TIMESTAMP")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                    .IsRequired()
                    .IsConcurrencyToken();

                entity.HasOne(i => i.Tenant)
                    .WithMany(t => t.Images)
                    .HasForeignKey(i => i.TenantId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("image_tenant_fk");

                entity.HasIndex(i => i.TenantId)
                    .HasDatabaseName("idx_tenant");

                entity.HasIndex(i => i.DeletedAt)
                    .HasDatabaseName("idx_deleted");

                entity.HasIndex(i => new { i.TenantId, i.DeletedAt })
                    .HasDatabaseName("idx_tenant_deleted");

                entity.HasQueryFilter(i => i.DeletedAt == null);
            }
        );

        builder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");

            entity.HasKey(o => o.Id);

            // Properties
            entity.Property(o => o.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(o => o.TenantId)
                .HasColumnName("tenant_id")
                .IsRequired();

            entity.Property(o => o.ShippingZoneId)
                .HasColumnName("shipping_zone_id")
                .IsRequired();

            entity.Property(o => o.CustomerId)
                .HasColumnName("customer_id")
                .IsRequired(false);

            entity.Property(o => o.OrderNumber)
                .HasColumnName("order_number")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(o => o.CustomerEmail)
                .HasColumnName("customer_email")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(o => o.CustomerFirstName)
                .HasColumnName("customer_first_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(o => o.CustomerLastName)
                .HasColumnName("customer_last_name")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(o => o.CustomerPhone)
                .HasColumnName("customer_phone")
                .HasMaxLength(50)
                .IsRequired(false);

            // Status tracking - all with string conversion for ENUMs
            entity.Property(o => o.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasDefaultValue(OrderStatus.Pending)
                .IsRequired();

            entity.Property(o => o.PaymentStatus)
                .HasColumnName("payment_status")
                .HasConversion<string>()
                .HasDefaultValue(PaymentStatus.Unpaid)
                .IsRequired();

            entity.Property(o => o.FulfillmentStatus)
                .HasColumnName("fulfillment_status")
                .HasConversion<string>()
                .HasDefaultValue(FulfillmentStatus.Unfulfilled)
                .IsRequired();

            // Amounts (in cents)
            entity.Property(o => o.SubtotalCents)
                .HasColumnName("subtotal_cents")
                .IsRequired();

            entity.Property(o => o.DiscountCents)
                .HasColumnName("discount_cents")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(o => o.CouponCode)
                .HasColumnName("coupon_code")
                .HasMaxLength(100)
                .IsRequired(false);

            entity.Property(o => o.CouponDiscountCents)
                .HasColumnName("coupon_discount_cents")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(o => o.TaxCents)
                .HasColumnName("tax_cents")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(o => o.ShippingCents)
                .HasColumnName("shipping_cents")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(o => o.TotalCents)
                .HasColumnName("total_cents")
                .IsRequired();

            entity.Property(o => o.RefundedAmountCents)
                .HasColumnName("refunded_amount_cents")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(o => o.PaymentServiceFeeCents)
                .HasColumnName("payment_service_fee_cents")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(o => o.Currency)
                .HasColumnName("currency")
                .HasMaxLength(20)
                .IsRequired();

            // Shipping address
            entity.Property(o => o.ShippingAddressLine1)
                .HasColumnName("shipping_address_line1")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(o => o.ShippingAddressLine2)
                .HasColumnName("shipping_address_line2")
                .HasMaxLength(255)
                .IsRequired(false);

            entity.Property(o => o.ShippingCity)
                .HasColumnName("shipping_city")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(o => o.ShippingState)
                .HasColumnName("shipping_state")
                .HasMaxLength(100)
                .IsRequired(false);

            entity.Property(o => o.ShippingPostalCode)
                .HasColumnName("shipping_postal_code")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(o => o.ShippingCountry)
                .HasColumnName("shipping_country")
                .HasMaxLength(100)
                .IsRequired();

            // Billing address
            entity.Property(o => o.BillingAddressLine1)
                .HasColumnName("billing_address_line1")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(o => o.BillingAddressLine2)
                .HasColumnName("billing_address_line2")
                .HasMaxLength(255)
                .IsRequired(false);

            entity.Property(o => o.BillingCity)
                .HasColumnName("billing_city")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(o => o.BillingState)
                .HasColumnName("billing_state")
                .HasMaxLength(100)
                .IsRequired(false);

            entity.Property(o => o.BillingPostalCode)
                .HasColumnName("billing_postal_code")
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(o => o.BillingCountry)
                .HasColumnName("billing_country")
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(o => o.OrderType)
                .HasColumnName("order_type")
                .HasConversion<string>()
                .HasDefaultValue(OrderType.Standard)
                .IsRequired();

            // Audit
            entity.Property(o => o.IpAddress)
                .HasColumnName("ip_address")
                .HasMaxLength(45)
                .IsRequired(false);

            entity.Property(o => o.UserAgent)
                .HasColumnName("user_agent")
                .HasColumnType("TEXT")
                .IsRequired(false);


            // Notes
            entity.Property(o => o.CustomerNotes)
                .HasColumnName("customer_notes")
                .HasColumnType("TEXT")
                .IsRequired(false);

            entity.Property(o => o.AdminNotes)
                .HasColumnName("admin_notes")
                .HasColumnType("TEXT")
                .IsRequired(false);

            // Timestamps
            entity.Property(o => o.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(o => o.UpdatedAt)
                .HasColumnName("updated_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(o => o.DeletedAt)
                .HasColumnName("deleted_at")
                .IsRequired(false);

            entity.Property(o => o.RowVersion)
                .HasColumnName("row_version")
                .IsRequired()
                .IsConcurrencyToken();

            // Relationships
            entity.HasOne(o => o.Tenant)
                .WithMany(t => t.Orders)
                .HasForeignKey(o => o.TenantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("orders_tenant_fk");

            entity.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("orders_customer_fk");

            entity.HasOne(o => o.ShippingZone)
                .WithMany(sz => sz.Orders)
                .HasForeignKey(o => o.ShippingZoneId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("orders_shipping_zone_fk");

            // Indexes
            entity.HasIndex(o => o.TenantId)
                .HasDatabaseName("idx_tenant");

            entity.HasIndex(o => o.OrderNumber)
                .HasDatabaseName("idx_order_number");

            entity.HasIndex(o => o.CustomerId)
                .HasDatabaseName("idx_customer_id");

            entity.HasIndex(o => o.CustomerEmail)
                .HasDatabaseName("idx_customer_email");

            entity.HasIndex(o => o.Status)
                .HasDatabaseName("idx_status");

            entity.HasIndex(o => o.PaymentStatus)
                .HasDatabaseName("idx_payment_status");

            entity.HasIndex(o => o.CreatedAt)
                .HasDatabaseName("idx_created_at");

            entity.HasIndex(o => new { o.TenantId, o.Status })
                .HasDatabaseName("idx_tenant_status");

            entity.HasIndex(o => new { o.TenantId, o.CreatedAt })
                .HasDatabaseName("idx_tenant_created");

            entity.HasIndex(o => new { o.TenantId, o.CustomerId })
                .HasDatabaseName("idx_tenant_customer");

            entity.HasIndex(o => o.DeletedAt)
                .HasDatabaseName("idx_deleted");

            entity.HasIndex(o => o.CreatedAt)
                .HasDatabaseName("idx_orders_stale_pending");

            entity.HasIndex(o => new { o.CustomerId, o.CreatedAt, o.Status })
                .HasDatabaseName("idx_customer_orders");
        });

        builder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("order_items");
            entity.HasKey(o => new { o.Id, o.TenantId }); // Composite key

            entity.Property(o => o.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(o => o.TenantId)
                .HasColumnName("tenant_id")
                .IsRequired();

            entity.Property(o => o.OrderId)
                .HasColumnName("order_id")
                .IsRequired();

            entity.Property(o => o.ProductId)
                .HasColumnName("product_id")
                .IsRequired();

            entity.Property(o => o.TaxCategoryId)
                .HasColumnName("tax_category_id");

            entity.Property(o => o.WeightGrams)
                .HasColumnName("weight_grams");

            entity.Property(o => o.ProductName)
                .HasColumnName("product_name")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(o => o.ProductSku)
                .HasColumnName("product_sku")
                .HasMaxLength(100);

            entity.Property(o => o.Quantity)
                .HasColumnName("quantity")
                .IsRequired();

            entity.Property(o => o.UnitPriceCents)
                .HasColumnName("unit_price_cents")
                .IsRequired();

            entity.Property(o => o.TotalPriceCents)
                .HasColumnName("total_price_cents")
                .IsRequired();

            entity.Property(o => o.DiscountCents)
                .HasColumnName("discount_cents")
                .HasDefaultValue(0)
                .IsRequired();

            entity.Property(o => o.OptionDetails)
                .HasColumnName("option_details")
                .HasColumnType("json");

            entity.Property(o => o.ImageUrl)
                .HasColumnName("image_url")
                .HasMaxLength(500);

            entity.Property(o => o.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("TIMESTAMP")
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(o => o.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("TIMESTAMP")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(o => o.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("TIMESTAMP");

            // Foreign Key Constraints
            entity.HasOne(o => o.Tenant)
                .WithMany()
                .HasForeignKey(o => o.TenantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("order_items_tenant_fk");

            entity.HasOne(o => o.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(o => new { o.OrderId, o.TenantId }) // Both columns
                .HasPrincipalKey(o => new { o.Id, o.TenantId }) // Explicitly specify principal key
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("order_items_order_fk");

            entity.HasOne(o => o.Product)
                .WithMany()
                .HasForeignKey(o => new { o.ProductId, o.TenantId })
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("order_items_product_fk");

            entity.HasOne(o => o.TaxCategory)
                .WithMany(tc => tc.OrderItems)
                .HasForeignKey(o => o.TaxCategoryId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("order_items_tax_category_fk");

            // Indexes
            entity.HasIndex(o => o.TenantId)
                .HasDatabaseName("idx_tenant");

            entity.HasIndex(o => o.OrderId)
                .HasDatabaseName("idx_order_id");

            entity.HasIndex(o => o.ProductId)
                .HasDatabaseName("idx_product_id");

            entity.HasIndex(o => o.DeletedAt)
                .HasDatabaseName("idx_deleted");

            entity.HasIndex(o => o.WeightGrams)
                .HasDatabaseName("idx_order_items_weight");
        });

        builder.Entity<PaymentTransactions>(entity =>
        {
            entity.ToTable("payment_transactions");

            // Primary key
            entity.HasKey(pt => pt.Id);

            entity.Property(pt => pt.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(pt => pt.TenantId)
                .HasColumnName("tenant_id")
                .IsRequired();

            entity.Property(pt => pt.OrderId)
                .HasColumnName("order_id")
                .IsRequired();

            entity.Property(pt => pt.AmountCents)
                .HasColumnName("amount_cents")
                .IsRequired();

            entity.Property(pt => pt.TransactionType)
                .HasColumnName("transaction_type")
                .HasConversion<string>()
                .HasDefaultValue(TransactionType.Authorize)
                .IsRequired();

            entity.Property(pt => pt.Currency)
                .HasColumnName("currency")
                .HasConversion<string>()
                .HasDefaultValue(Currency.Usd)
                .IsRequired();

            entity.Property(pt => pt.GatewayName)
                .HasColumnName("gateway_name")
                .HasConversion<string>()
                .HasDefaultValue(GatewayName.Stripe)
                .IsRequired();

            entity.Property(pt => pt.GatewayTransactionId)
                .HasColumnName("gateway_transaction_id")
                .HasMaxLength(255)
                .IsRequired(false);

            entity.Property(pt => pt.GatewayFeeCents)
                .HasColumnName("gateway_fee_cents")
                .IsRequired();

            entity.Property(pt => pt.Status)
                .HasColumnName("status")
                .HasConversion<string>()
                .HasDefaultValue(PaymentTransactionStatus.Pending)
                .IsRequired();

            entity.Property(pt => pt.ErrorCode)
                .HasColumnName("error_code")
                .HasMaxLength(100)
                .IsRequired(false);

            entity.Property(pt => pt.RawGatewayResponse)
                .HasColumnName("raw_gateway_response")
                .HasColumnType("JSON")
                .IsRequired(false);

            entity.Property(pt => pt.ErrorMessage)
                .HasColumnName("error_message")
                .HasColumnType("TEXT")
                .IsRequired(false);

            // Timestamps
            entity.Property(pt => pt.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            entity.Property(pt => pt.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)")
                .IsRequired();

            entity.Property(pt => pt.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("DATETIME(6)")
                .IsRequired(false);

            // Concurrency
            entity.Property(pt => pt.RowVersion)
                .HasColumnName("row_version")
                .HasColumnType("TIMESTAMP")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                .IsRequired()
                .IsConcurrencyToken();

            // Relationships
            entity.HasOne(pt => pt.Tenant)
                .WithMany(t => t.PaymentTransactions)
                .HasForeignKey(pt => pt.TenantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("payment_transactions_tenant_fk");

            entity.HasOne(pt => pt.Order)
                .WithMany(o => o.PaymentTransactions)
                .HasForeignKey(pt => pt.OrderId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("payment_transactions_order_fk");

            // Indexes
            entity.HasIndex(pt => pt.TenantId)
                .HasDatabaseName("idx_tenant");

            entity.HasIndex(pt => pt.OrderId)
                .HasDatabaseName("idx_order_id");

            entity.HasIndex(pt => pt.Status)
                .HasDatabaseName("idx_status");

            entity.HasIndex(pt => pt.CreatedAt)
                .HasDatabaseName("idx_created_at");
        });

        builder.Entity<Customer>(entity =>
        {
            entity.ToTable("customers");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd()
                .IsRequired();
            entity.Property(c => c.TenantId)
                .HasColumnName("tenant_id")
                .IsRequired();


            entity.Property(c => c.Email)
                .HasColumnName("email")
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(c => c.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            entity.Property(c => c.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            entity.Property(c => c.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("DATETIME(6)")
                .IsRequired(false);

            entity.Property(c => c.RowVersion)
                .HasColumnName("row_version")
                .HasColumnType("TIMESTAMP")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                .IsRequired()
                .IsConcurrencyToken();

            // Relationships
            entity.HasOne(c => c.Tenant)
                .WithMany(t => t.Customers)
                .HasForeignKey(c => c.TenantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("customer_tenant_fk");

            entity.HasMany(c => c.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Indexes
            entity.HasIndex(c => new { c.TenantId, c.Email })
                .IsUnique()
                .HasDatabaseName("uq_customers_tenant_email");

            entity.HasIndex(c => c.TenantId)
                .HasDatabaseName("idx_tenant_id");

            entity.HasIndex(c => c.Email)
                .HasDatabaseName("idx_email");

            entity.HasIndex(c => c.DeletedAt)
                .HasDatabaseName("idx_deleted_at");
        });

        builder.Entity<TaxCategory>(entity =>
            {
                entity.ToTable("tax_categories");
                entity.HasKey(tc => tc.Id);
                entity.Property(tc => tc.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd()
                    .IsRequired();
                entity.Property(tc => tc.TenantId)
                    .HasColumnName("tenant_id")
                    .IsRequired();
                entity.Property(tc => tc.Name)
                    .HasColumnName("name")
                    .HasMaxLength(255);
                entity.Property(tc => tc.TaxRate)
                    .HasColumnName("tax_rate")
                    .IsRequired();

                entity.Property(c => c.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("DATETIME(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .IsRequired();

                entity.Property(c => c.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("DATETIME(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .IsRequired();

                entity.Property(c => c.DeletedAt)
                    .HasColumnName("deleted_at")
                    .HasColumnType("DATETIME(6)")
                    .IsRequired(false);

                entity.Property(c => c.RowVersion)
                    .HasColumnName("row_version")
                    .HasColumnType("TIMESTAMP")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                    .IsRequired()
                    .IsConcurrencyToken();

                entity.HasOne(c => c.Tenant)
                    .WithMany(t => t.TaxCategories)
                    .HasForeignKey(c => c.TenantId);

                entity.HasMany(e => e.OrderItems)
                    .WithOne(e => e.TaxCategory)
                    .HasForeignKey(e => e.TaxCategoryId)
                    .OnDelete(DeleteBehavior.Restrict);
            }
        );

        builder.Entity<ShippingZone>(entity =>
            {
                entity.ToTable("shipping_zones");
                entity.HasKey(sz => sz.Id);
                entity.Property(sz => sz.Id)
                    .HasColumnName("id")
                    .ValueGeneratedOnAdd()
                    .IsRequired();
                entity.Property(sz => sz.TenantId)
                    .HasColumnName("tenant_id")
                    .IsRequired();
                entity.Property(sz => sz.Name)
                    .HasMaxLength(100)
                    .IsRequired();
                entity.Property(sz => sz.IsLocalFleet)
                    .HasColumnName("is_local_fleet")
                    .HasDefaultValue(false)
                    .IsRequired();
                entity.Property(c => c.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("DATETIME(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .IsRequired();

                entity.Property(c => c.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("DATETIME(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                    .IsRequired();

                entity.Property(c => c.DeletedAt)
                    .HasColumnName("deleted_at")
                    .HasColumnType("DATETIME(6)")
                    .IsRequired(false);

                entity.Property(c => c.RowVersion)
                    .HasColumnName("row_version")
                    .HasColumnType("TIMESTAMP")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                    .IsRequired()
                    .IsConcurrencyToken();

                // Relationships
                entity.HasOne(sz => sz.Tenant)
                    .WithMany(t => t.ShippingZones)
                    .HasForeignKey(sz => sz.TenantId)
                    .OnDelete(DeleteBehavior.Cascade)
                    .HasConstraintName("zone_tenant_fk");

                // Navigation collections
                entity.HasMany(sz => sz.ZonePostalCodes)
                    .WithOne(zpc => zpc.ShippingZone)
                    .HasForeignKey(zpc => zpc.ShippingZoneId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(sz => sz.ShippingRates)
                    .WithOne(sr => sr.ShippingZone)
                    .HasForeignKey(sr => sr.ShippingZoneId)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        );

        builder.Entity<ZonePostalCode>(entity =>
        {
            entity.ToTable("zone_postal_codes");

            // Primary key
            entity.HasKey(zpc => zpc.Id);

            entity.Property(zpc => zpc.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(zpc => zpc.TenantId)
                .HasColumnName("tenant_id")
                .IsRequired();

            entity.Property(zpc => zpc.ShippingZoneId)
                .HasColumnName("shipping_zone_id")
                .IsRequired();

            entity.Property(zpc => zpc.PostalCode)
                .HasColumnName("postal_code")
                .HasMaxLength(20)
                .IsRequired();

            // Timestamps
            entity.Property(zpc => zpc.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            entity.Property(zpc => zpc.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            entity.Property(zpc => zpc.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("DATETIME(6)")
                .IsRequired(false);

            // Concurrency
            entity.Property(zpc => zpc.RowVersion)
                .HasColumnName("row_version")
                .HasColumnType("TIMESTAMP")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                .IsRequired()
                .IsConcurrencyToken();

            // Relationships
            entity.HasOne(zpc => zpc.Tenant)
                .WithMany(t => t.ZonePostalCodes)
                .HasForeignKey(zpc => zpc.TenantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("zone_postal_code_tenant_fk");

            entity.HasOne(zpc => zpc.ShippingZone)
                .WithMany(sz => sz.ZonePostalCodes)
                .HasForeignKey(zpc => zpc.ShippingZoneId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("zone_postal_code_shipping_zone_fk");

            // Indexes
            entity.HasIndex(zpc => zpc.ShippingZoneId)
                .HasDatabaseName("idx_shipping_zone");

            entity.HasIndex(zpc => new { zpc.TenantId, zpc.PostalCode })
                .HasDatabaseName("idx_postal_code_lookup");
        });

        builder.Entity<ShippingRate>(entity =>
        {
            entity.ToTable("shipping_rates");

            // Primary key
            entity.HasKey(sr => sr.Id);

            entity.Property(sr => sr.Id)
                .HasColumnName("id")
                .ValueGeneratedOnAdd()
                .IsRequired();

            entity.Property(sr => sr.TenantId)
                .HasColumnName("tenant_id")
                .IsRequired();

            entity.Property(sr => sr.ShippingZoneId)
                .HasColumnName("shipping_zone_id")
                .IsRequired();

            entity.Property(sr => sr.RateName)
                .HasColumnName("rate_name")
                .HasMaxLength(100)
                .IsRequired(false);

            entity.Property(sr => sr.MinWeight)
                .HasColumnName("min_weight")
                .HasPrecision(10, 2)
                .HasDefaultValue(0.00m)
                .IsRequired();

            entity.Property(sr => sr.MaxWeight)
                .HasColumnName("max_weight")
                .HasPrecision(10, 2)
                .IsRequired();

            entity.Property(sr => sr.PriceCents)
                .HasColumnName("priceCents")
                .IsRequired();

            // Timestamps
            entity.Property(sr => sr.CreatedAt)
                .HasColumnName("created_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            entity.Property(sr => sr.UpdatedAt)
                .HasColumnName("updated_at")
                .HasColumnType("DATETIME(6)")
                .HasDefaultValueSql("CURRENT_TIMESTAMP(6)")
                .IsRequired();

            entity.Property(sr => sr.DeletedAt)
                .HasColumnName("deleted_at")
                .HasColumnType("DATETIME(6)")
                .IsRequired(false);

            // Concurrency
            entity.Property(sr => sr.RowVersion)
                .HasColumnName("row_version")
                .HasColumnType("TIMESTAMP")
                .HasDefaultValueSql("CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP")
                .IsRequired()
                .IsConcurrencyToken();

            // Relationships
            entity.HasOne(sr => sr.Tenant)
                .WithMany(t => t.ShippingRates)
                .HasForeignKey(sr => sr.TenantId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("shipping_rate_tenant_fk");

            entity.HasOne(sr => sr.ShippingZone)
                .WithMany(sz => sz.ShippingRates)
                .HasForeignKey(sr => sr.ShippingZoneId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("shipping_rate_shipping_zone_fk");

            // Indexes
            entity.HasIndex(sr => sr.ShippingZoneId)
                .HasDatabaseName("idx_shipping_zone");

            entity.HasIndex(sr => new { sr.TenantId, sr.ShippingZoneId, sr.MinWeight, sr.MaxWeight })
                .HasDatabaseName("idx_rate_weight_range");
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