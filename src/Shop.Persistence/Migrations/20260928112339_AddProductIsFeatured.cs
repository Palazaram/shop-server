using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProductIsFeatured : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_featured",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "ix_products_is_featured",
                table: "products",
                column: "is_featured",
                filter: "is_featured");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_products_is_featured",
                table: "products");

            migrationBuilder.DropColumn(
                name: "is_featured",
                table: "products");
        }
    }
}
