using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class GlobalCategorySlugUniqueness : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_categories_parent_id_slug",
                table: "categories");

            migrationBuilder.DropIndex(
                name: "ix_categories_slug",
                table: "categories");

            migrationBuilder.CreateIndex(
                name: "ix_categories_slug",
                table: "categories",
                column: "slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_categories_slug",
                table: "categories");

            migrationBuilder.CreateIndex(
                name: "ix_categories_parent_id_slug",
                table: "categories",
                columns: new[] { "parent_id", "slug" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_categories_slug",
                table: "categories",
                column: "slug",
                unique: true,
                filter: "parent_id IS NULL");
        }
    }
}
