using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ManufacturerCountryReference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "country_id",
                table: "manufacturers",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE manufacturers AS m
                SET country_id = c.id
                FROM countries AS c
                WHERE c.name = m.country;
                """);

            migrationBuilder.Sql("""
                DO $$
                DECLARE orphans int;
                BEGIN
                    SELECT count(*) INTO orphans FROM manufacturers WHERE country_id IS NULL;

                    IF orphans > 0 THEN
                        RAISE EXCEPTION 'Manufacturers without a matching country: %', orphans;
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "country_id",
                table: "manufacturers",
                type: "uuid",
                nullable: false);

            migrationBuilder.DropColumn(name: "country", table: "manufacturers");

            migrationBuilder.CreateIndex(
                name: "ix_manufacturers_country_id",
                table: "manufacturers",
                column: "country_id");

            migrationBuilder.AddForeignKey(
                name: "fk_manufacturers_countries_country_id",
                table: "manufacturers",
                column: "country_id",
                principalTable: "countries",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "country",
                table: "manufacturers",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE manufacturers AS m
                SET country = c.name
                FROM countries AS c
                WHERE c.id = m.country_id;
                """);

            migrationBuilder.DropForeignKey(
                name: "fk_manufacturers_countries_country_id", table: "manufacturers");

            migrationBuilder.DropIndex(name: "ix_manufacturers_country_id", table: "manufacturers");

            migrationBuilder.DropColumn(name: "country_id", table: "manufacturers");
        }
    }
}
