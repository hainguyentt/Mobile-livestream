using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LivestreamApp.API.Migrations
{
    /// <inheritdoc />
    public partial class ChangeInterestsToTextArray : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // PostgreSQL does not allow subqueries in ALTER COLUMN USING.
            // Strategy: add new text[] column, populate from jsonb, drop old, rename.
            migrationBuilder.Sql("""
                ALTER TABLE user_profiles ADD COLUMN "Interests_new" text[] NOT NULL DEFAULT '{}';
                UPDATE user_profiles SET "Interests_new" = ARRAY(SELECT jsonb_array_elements_text("Interests"));
                ALTER TABLE user_profiles DROP COLUMN "Interests";
                ALTER TABLE user_profiles RENAME COLUMN "Interests_new" TO "Interests";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert text[] → jsonb
            migrationBuilder.Sql("""
                ALTER TABLE user_profiles ADD COLUMN "Interests_old" jsonb NOT NULL DEFAULT '[]';
                UPDATE user_profiles SET "Interests_old" = to_jsonb("Interests");
                ALTER TABLE user_profiles DROP COLUMN "Interests";
                ALTER TABLE user_profiles RENAME COLUMN "Interests_old" TO "Interests";
                """);
        }
    }
}
