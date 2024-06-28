using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Projeto.Data.Migrations
{
    /// <inheritdoc />
    public partial class alteracoes4Reviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Categories_Category",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Reviews",
                newName: "CategoryFK");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_Category",
                table: "Reviews",
                newName: "IX_Reviews_CategoryFK");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Categories_CategoryFK",
                table: "Reviews",
                column: "CategoryFK",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Categories_CategoryFK",
                table: "Reviews");

            migrationBuilder.RenameColumn(
                name: "CategoryFK",
                table: "Reviews",
                newName: "Category");

            migrationBuilder.RenameIndex(
                name: "IX_Reviews_CategoryFK",
                table: "Reviews",
                newName: "IX_Reviews_Category");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Categories_Category",
                table: "Reviews",
                column: "Category",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
