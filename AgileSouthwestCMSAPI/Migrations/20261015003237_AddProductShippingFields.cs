using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace AgileSouthwestCMSAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddProductShippingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cms_users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    cognito_user_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    role = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    deleted_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cms_users", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    sub_domain = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    custom_domain = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    created_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    deleted_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: true),
                    row_version = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tenants", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    deleted_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: true),
                    row_version = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn),
                    CmsUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.id);
                    table.ForeignKey(
                        name: "FK_customers_cms_users_CmsUserId",
                        column: x => x.CmsUserId,
                        principalTable: "cms_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "customer_tenant_fk",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "images",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    url = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: false),
                    original_filename = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    file_size = table.Column<long>(type: "bigint", nullable: true),
                    content_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    created_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    deleted_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: true),
                    row_version = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_images", x => x.id);
                    table.ForeignKey(
                        name: "image_tenant_fk",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "shipping_rates",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    rate_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    postal_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    min_weight_grams = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    max_weight_grams = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    price_cents = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    deleted_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: true),
                    row_version = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_shipping_rates", x => x.id);
                    table.ForeignKey(
                        name: "shipping_rate_tenant_fk",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "stores",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    sub_domain = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    is_online = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    deleted_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: true),
                    row_version = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stores", x => x.id);
                    table.ForeignKey(
                        name: "stores_tenant_id_fk",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tax_categories",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    tax_rate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    created_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    deleted_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: true),
                    row_version = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tax_categories", x => x.id);
                    table.ForeignKey(
                        name: "FK_tax_categories_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user_tenants",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "int", nullable: false),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    Role = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    created_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    deleted_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: true),
                    row_version = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_tenants", x => new { x.tenant_id, x.user_id });
                    table.ForeignKey(
                        name: "user_tenant_tenant_fk",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_tenant_user_fk",
                        column: x => x.user_id,
                        principalTable: "cms_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    shipping_rate_id = table.Column<int>(type: "int", nullable: true),
                    customer_id = table.Column<int>(type: "int", nullable: true),
                    order_number = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    idempotency_key = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true),
                    customer_email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    customer_first_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    customer_last_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    customer_phone = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true),
                    status = table.Column<string>(type: "varchar(255)", nullable: false, defaultValue: "Pending"),
                    payment_status = table.Column<string>(type: "varchar(255)", nullable: false, defaultValue: "Unpaid"),
                    fulfillment_status = table.Column<string>(type: "longtext", nullable: true),
                    subtotal_cents = table.Column<int>(type: "int", nullable: false),
                    discount_cents = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    coupon_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    coupon_discount_cents = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    tax_cents = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    shipping_cents = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    total_cents = table.Column<int>(type: "int", nullable: false),
                    refunded_amount_cents = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    payment_service_fee_cents = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    currency = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    shipping_address_line1 = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    shipping_address_line2 = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    shipping_city = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    shipping_state = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    shipping_postal_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    shipping_country = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    billing_address_line1 = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    billing_address_line2 = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    billing_city = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    billing_state = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    billing_postal_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    billing_country = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    order_type = table.Column<string>(type: "longtext", nullable: false, defaultValue: "Standard"),
                    ip_address = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true),
                    user_agent = table.Column<string>(type: "TEXT", nullable: true),
                    customer_notes = table.Column<string>(type: "TEXT", nullable: true),
                    admin_notes = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)"),
                    deleted_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: true),
                    row_version = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_orders", x => x.id);
                    table.ForeignKey(
                        name: "orders_customer_fk",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "orders_shipping_rate_fk",
                        column: x => x.shipping_rate_id,
                        principalTable: "shipping_rates",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "orders_tenant_fk",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    tax_category_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "longtext", nullable: false),
                    base_price_cents = table.Column<int>(type: "int", nullable: false),
                    weight_grams = table.Column<int>(type: "int", nullable: false),
                    requires_shipping = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    is_enhanced_payment_required = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    deleted_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: true),
                    row_version = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products", x => x.id);
                    table.CheckConstraint("CK_products_weight_grams", "weight_grams >= 0");
                    table.ForeignKey(
                        name: "FK_products_tax_categories_tax_category_id",
                        column: x => x.tax_category_id,
                        principalTable: "tax_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_products_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "order_status_history",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    changed_by_id = table.Column<int>(type: "int", nullable: false),
                    new_status = table.Column<string>(type: "longtext", nullable: false),
                    old_status = table.Column<string>(type: "longtext", nullable: false),
                    new_payment_status = table.Column<string>(type: "longtext", nullable: true),
                    old_payment_status = table.Column<string>(type: "longtext", nullable: true),
                    new_fulfillment_status = table.Column<string>(type: "longtext", nullable: true),
                    old_fulfillment_status = table.Column<string>(type: "longtext", nullable: true),
                    reason = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    created_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    deleted_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: true),
                    row_version = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_status_history", x => x.id);
                    table.ForeignKey(
                        name: "order_status_history_order_fk",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "order_status_history_tenant_id_fk",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "orders_status_history_changed_by_id_fk",
                        column: x => x.changed_by_id,
                        principalTable: "cms_users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "payment_transactions",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    amount_cents = table.Column<int>(type: "int", nullable: false),
                    transaction_type = table.Column<string>(type: "longtext", nullable: false, defaultValue: "Authorize"),
                    currency = table.Column<string>(type: "longtext", nullable: false, defaultValue: "Usd"),
                    gateway_name = table.Column<string>(type: "longtext", nullable: false, defaultValue: "Stripe"),
                    gateway_transaction_id = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    gateway_fee_cents = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "varchar(255)", nullable: false, defaultValue: "Pending"),
                    error_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    raw_gateway_response = table.Column<string>(type: "JSON", nullable: true),
                    error_message = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)"),
                    deleted_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: true),
                    row_version = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_transactions", x => x.id);
                    table.ForeignKey(
                        name: "payment_transactions_order_fk",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "payment_transactions_tenant_fk",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "inventory",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    store_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    deleted_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: true),
                    row_version = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inventory", x => x.id);
                    table.CheckConstraint("CK_inventory_quantity", "quantity >= 0");
                    table.ForeignKey(
                        name: "inventory_product_tenant_id_fk",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "inventory_store_fk",
                        column: x => x.store_id,
                        principalTable: "stores",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "inventory_tenant_id_fk",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    order_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    tax_category_id = table.Column<int>(type: "int", nullable: true),
                    product_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    product_sku = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    unit_price_cents = table.Column<int>(type: "int", nullable: false),
                    total_price_cents = table.Column<int>(type: "int", nullable: false),
                    discount_cents = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    tax_cents = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    weight_grams = table.Column<int>(type: "int", nullable: true),
                    option_details = table.Column<string>(type: "JSON", nullable: true),
                    image_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6)"),
                    deleted_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: true),
                    row_version = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_order_items", x => x.id);
                    table.ForeignKey(
                        name: "order_items_order_fk",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "order_items_product_fk",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "order_items_tax_category_fk",
                        column: x => x.tax_category_id,
                        principalTable: "tax_categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "order_items_tenant_fk",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "product_images",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    image_id = table.Column<int>(type: "int", nullable: false),
                    is_primary = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    position = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    created_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    deleted_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: true),
                    RowVersion = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_images_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_product_images_image",
                        column: x => x.image_id,
                        principalTable: "images",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_product_images_product",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "product_options",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    product_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    is_required = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    deleted_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: true),
                    row_version = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_options", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_options_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_options_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "product_option_choices",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    tenant_id = table.Column<int>(type: "int", nullable: false),
                    option_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    price_delta_cents = table.Column<int>(type: "int", nullable: false),
                    sale_price_delta_cents = table.Column<int>(type: "int", nullable: false),
                    is_active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    updated_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP(6)"),
                    deleted_at = table.Column<DateTime>(type: "DATETIME(6)", nullable: true),
                    row_version = table.Column<DateTime>(type: "datetime(6)", rowVersion: true, nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.ComputedColumn)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_option_choices", x => x.id);
                    table.ForeignKey(
                        name: "FK_product_option_choices_product_options_option_id",
                        column: x => x.option_id,
                        principalTable: "product_options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_product_option_choices_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "uq_cms_users_cognito_user_id",
                table: "cms_users",
                column: "cognito_user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uq_cms_users_email",
                table: "cms_users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_deleted_at",
                table: "customers",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "idx_email",
                table: "customers",
                column: "email");

            migrationBuilder.CreateIndex(
                name: "idx_tenant_id",
                table: "customers",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_customers_CmsUserId",
                table: "customers",
                column: "CmsUserId");

            migrationBuilder.CreateIndex(
                name: "uq_customers_tenant_email",
                table: "customers",
                columns: new[] { "tenant_id", "email" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_deleted",
                table: "images",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "idx_tenant",
                table: "images",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "idx_tenant_deleted",
                table: "images",
                columns: new[] { "tenant_id", "deleted_at" });

            migrationBuilder.CreateIndex(
                name: "inventory_tenant_deleted_idx",
                table: "inventory",
                columns: new[] { "tenant_id", "deleted_at" });

            migrationBuilder.CreateIndex(
                name: "inventory_tenant_id_idx",
                table: "inventory",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "inventory_tenant_product_idx",
                table: "inventory",
                columns: new[] { "tenant_id", "product_id" });

            migrationBuilder.CreateIndex(
                name: "inventory_tenant_quantity_idx",
                table: "inventory",
                columns: new[] { "tenant_id", "quantity" });

            migrationBuilder.CreateIndex(
                name: "inventory_tenant_store_idx",
                table: "inventory",
                columns: new[] { "tenant_id", "store_id" });

            migrationBuilder.CreateIndex(
                name: "inventory_tenant_store_product_uk",
                table: "inventory",
                columns: new[] { "tenant_id", "store_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inventory_product_id",
                table: "inventory",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_store_id",
                table: "inventory",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "idx_deleted",
                table: "order_items",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "idx_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "idx_product_id",
                table: "order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "idx_tenant",
                table: "order_items",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_order_items_tax_category_id",
                table: "order_items",
                column: "tax_category_id");

            migrationBuilder.CreateIndex(
                name: "idx_changed_by_id",
                table: "order_status_history",
                column: "changed_by_id");

            migrationBuilder.CreateIndex(
                name: "idx_order_id",
                table: "order_status_history",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "idx_tenant_id",
                table: "order_status_history",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "idx_created_at",
                table: "orders",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_customer_email",
                table: "orders",
                column: "customer_email");

            migrationBuilder.CreateIndex(
                name: "idx_customer_id",
                table: "orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "idx_customer_orders",
                table: "orders",
                columns: new[] { "customer_id", "created_at", "status" });

            migrationBuilder.CreateIndex(
                name: "idx_deleted",
                table: "orders",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "idx_orders_stale_pending",
                table: "orders",
                columns: new[] { "status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_payment_status",
                table: "orders",
                column: "payment_status");

            migrationBuilder.CreateIndex(
                name: "idx_status",
                table: "orders",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_tenant",
                table: "orders",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "idx_tenant_created",
                table: "orders",
                columns: new[] { "tenant_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "idx_tenant_customer",
                table: "orders",
                columns: new[] { "tenant_id", "customer_id" });

            migrationBuilder.CreateIndex(
                name: "idx_tenant_status",
                table: "orders",
                columns: new[] { "tenant_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_orders_shipping_rate_id",
                table: "orders",
                column: "shipping_rate_id");

            migrationBuilder.CreateIndex(
                name: "uk_tenant_idempotency_key",
                table: "orders",
                columns: new[] { "tenant_id", "idempotency_key" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_tenant_order_number",
                table: "orders",
                columns: new[] { "tenant_id", "order_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_created_at",
                table: "payment_transactions",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "idx_order_id",
                table: "payment_transactions",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "idx_status",
                table: "payment_transactions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_tenant",
                table: "payment_transactions",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "idx_deleted",
                table: "product_images",
                column: "deleted_at");

            migrationBuilder.CreateIndex(
                name: "image_primary_idx",
                table: "product_images",
                columns: new[] { "product_id", "is_primary" });

            migrationBuilder.CreateIndex(
                name: "image_product_idx",
                table: "product_images",
                columns: new[] { "product_id", "tenant_id" });

            migrationBuilder.CreateIndex(
                name: "image_product_tenant_idx",
                table: "product_images",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_product_images_image_id",
                table: "product_images",
                column: "image_id");

            migrationBuilder.CreateIndex(
                name: "uk_active_product_image",
                table: "product_images",
                columns: new[] { "tenant_id", "product_id", "image_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "uk_position_per_product",
                table: "product_images",
                columns: new[] { "tenant_id", "product_id", "position" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "product_option_choice_option_idx",
                table: "product_option_choices",
                columns: new[] { "option_id", "tenant_id" });

            migrationBuilder.CreateIndex(
                name: "product_option_choice_tenant_idx",
                table: "product_option_choices",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "product_option_product_idx",
                table: "product_options",
                columns: new[] { "product_id", "tenant_id" });

            migrationBuilder.CreateIndex(
                name: "product_option_tenant_idx",
                table: "product_options",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_products_tax_category_id",
                table: "products",
                column: "tax_category_id");

            migrationBuilder.CreateIndex(
                name: "product_tenant_active_idx",
                table: "products",
                columns: new[] { "tenant_id", "is_active" });

            migrationBuilder.CreateIndex(
                name: "product_tenant_idx",
                table: "products",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "product_tenant_name_idx",
                table: "products",
                columns: new[] { "tenant_id", "name" });

            migrationBuilder.CreateIndex(
                name: "shipping_rate_tenant_postal_idx",
                table: "shipping_rates",
                columns: new[] { "tenant_id", "postal_code" });

            migrationBuilder.CreateIndex(
                name: "stores_tenant_id_idx",
                table: "stores",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "stores_tenant_subdomain_uk",
                table: "stores",
                columns: new[] { "tenant_id", "sub_domain" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tax_categories_tenant_id",
                table: "tax_categories",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "uq_tenants_subdomain",
                table: "tenants",
                column: "sub_domain",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_tenants_tenant_id",
                table: "user_tenants",
                column: "tenant_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_tenants_user_id",
                table: "user_tenants",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inventory");

            migrationBuilder.DropTable(
                name: "order_items");

            migrationBuilder.DropTable(
                name: "order_status_history");

            migrationBuilder.DropTable(
                name: "payment_transactions");

            migrationBuilder.DropTable(
                name: "product_images");

            migrationBuilder.DropTable(
                name: "product_option_choices");

            migrationBuilder.DropTable(
                name: "user_tenants");

            migrationBuilder.DropTable(
                name: "stores");

            migrationBuilder.DropTable(
                name: "orders");

            migrationBuilder.DropTable(
                name: "images");

            migrationBuilder.DropTable(
                name: "product_options");

            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
                name: "shipping_rates");

            migrationBuilder.DropTable(
                name: "products");

            migrationBuilder.DropTable(
                name: "cms_users");

            migrationBuilder.DropTable(
                name: "tax_categories");

            migrationBuilder.DropTable(
                name: "tenants");
        }
    }
}
