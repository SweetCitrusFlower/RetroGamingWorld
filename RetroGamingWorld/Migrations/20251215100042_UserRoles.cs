using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetroGamingWorld.Migrations
{
    /// <inheritdoc />
    public partial class UserRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ArticleBookmarks",
                table: "ArticleBookmarks");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArticleBookmarks",
                table: "ArticleBookmarks",
                columns: new[] { "Id", "ArticleId", "BookmarkId" });

            migrationBuilder.CreateIndex(
                name: "IX_ArticleBookmarks_ArticleId",
                table: "ArticleBookmarks",
                column: "ArticleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ArticleBookmarks",
                table: "ArticleBookmarks");

            migrationBuilder.DropIndex(
                name: "IX_ArticleBookmarks_ArticleId",
                table: "ArticleBookmarks");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ArticleBookmarks",
                table: "ArticleBookmarks",
                columns: new[] { "ArticleId", "BookmarkId" });
        }
    }
}
