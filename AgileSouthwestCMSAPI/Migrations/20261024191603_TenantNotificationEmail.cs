using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgileSouthwestCMSAPI.Migrations
{
    /// <inheritdoc />
    public partial class TenantNotificationEmail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "notification_email",
                table: "tenants",
                type: "varchar(254)",
                maxLength: 254,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "notification_email",
                table: "tenants");
        }
    }
}
