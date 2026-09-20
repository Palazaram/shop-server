using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPackagingKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "packaging_key",
                table: "product_variants",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE product_variants
                SET packaging_key = trim_scale(packaging_value)::text || '-' || lower(packaging_unit);
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE product_variants ALTER COLUMN packaging_key DROP DEFAULT;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "packaging_key",
                table: "product_variants");
        }
    }
}
