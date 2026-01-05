using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetroGamingWorld.Migrations
{
    /// <inheritdoc />
    public partial class RemoveMethodRehaul : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. CREĂM TABELA NOUĂ PENTRU WISHLIST
            migrationBuilder.CreateTable(
                name: "UserWishlist",
                columns: table => new
                {
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WishlistId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    // Cheia primară compusă
                    table.PrimaryKey("PK_UserWishlist", x => new { x.ApplicationUserId, x.WishlistId });

                    // Relația cu Articolul
                    table.ForeignKey(
                        name: "FK_UserWishlist_Articles_WishlistId",
                        column: x => x.WishlistId,
                        principalTable: "Articles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);

                    // Relația cu Userul
                    table.ForeignKey(
                        name: "FK_UserWishlist_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            // 2. CREĂM INDEX DOAR PENTRU TABELA NOUĂ
            // (Am șters index-urile pentru ArticleBookmark, AspNetUsers etc.)
            migrationBuilder.CreateIndex(
                name: "IX_UserWishlist_WishlistId",
                table: "UserWishlist",
                column: "WishlistId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // DACĂ ANULĂM MIGRAREA, ȘTERGEM DOAR WISHLIST-UL
            // (Nu ștergem Userii sau Articolele!)
            migrationBuilder.DropTable(
                name: "UserWishlist");
        }
    }
}