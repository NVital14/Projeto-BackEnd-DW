using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projeto.Migrations
{
    /// <inheritdoc />
    public partial class alteracoesFavorites : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Reviews_ReviewId",
                table: "Favorites");

            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Utilizadores_UserId",
                table: "Favorites");

            migrationBuilder.RenameColumn(
                name: "ReviewId",
                table: "Favorites",
                newName: "ReviewFK");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Favorites",
                newName: "UtilizadorFK");

            migrationBuilder.RenameIndex(
                name: "IX_Favorites_ReviewId",
                table: "Favorites",
                newName: "IX_Favorites_ReviewFK");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Reviews_ReviewFK",
                table: "Favorites",
                column: "ReviewFK",
                principalTable: "Reviews",
                principalColumn: "ReviewId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Utilizadores_UtilizadorFK",
                table: "Favorites",
                column: "UtilizadorFK",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Reviews_ReviewFK",
                table: "Favorites");

            migrationBuilder.DropForeignKey(
                name: "FK_Favorites_Utilizadores_UtilizadorFK",
                table: "Favorites");

            migrationBuilder.RenameColumn(
                name: "ReviewFK",
                table: "Favorites",
                newName: "ReviewId");

            migrationBuilder.RenameColumn(
                name: "UtilizadorFK",
                table: "Favorites",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Favorites_ReviewFK",
                table: "Favorites",
                newName: "IX_Favorites_ReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Reviews_ReviewId",
                table: "Favorites",
                column: "ReviewId",
                principalTable: "Reviews",
                principalColumn: "ReviewId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Favorites_Utilizadores_UserId",
                table: "Favorites",
                column: "UserId",
                principalTable: "Utilizadores",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
