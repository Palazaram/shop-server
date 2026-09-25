using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryDisplayOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "display_order",
                table: "categories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("""
                WITH ordered AS (
                    SELECT id,
                           ROW_NUMBER() OVER (PARTITION BY parent_id ORDER BY name) - 1 AS position
                    FROM categories)
                UPDATE categories AS c
                SET display_order = ordered.position
                FROM ordered
                WHERE c.id = ordered.id;
                """);

            migrationBuilder.Sql("ALTER TABLE categories ALTER COLUMN display_order DROP DEFAULT;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "display_order",
                table: "categories");
        }
    }
}
