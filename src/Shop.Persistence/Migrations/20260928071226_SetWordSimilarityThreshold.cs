using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shop.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SetWordSimilarityThreshold : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Порог для оператора <%. Выбран по замерам на тестовом каталоге:
            // нижняя граница нужных совпадений 0.444, верхняя граница шума 0.429.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    EXECUTE format(
                        'ALTER DATABASE %I SET pg_trgm.word_similarity_threshold = 0.44',
                        current_database());
                END $$;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    EXECUTE format(
                        'ALTER DATABASE %I RESET pg_trgm.word_similarity_threshold',
                        current_database());
                END $$;
                """);
        }
    }
}
