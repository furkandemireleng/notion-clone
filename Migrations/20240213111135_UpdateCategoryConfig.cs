using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace notion_clone.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCategoryConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Category_Post_PostEntityId",
                table: "Category");

            migrationBuilder.DropIndex(
                name: "IX_Category_PostEntityId",
                table: "Category");

            migrationBuilder.DropColumn(
                name: "PostEntityId",
                table: "Category");

            migrationBuilder.CreateTable(
                name: "CategoryEntityPostEntity",
                columns: table => new
                {
                    CategoriesId = table.Column<Guid>(type: "uuid", nullable: false),
                    PostEntityId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoryEntityPostEntity", x => new { x.CategoriesId, x.PostEntityId });
                    table.ForeignKey(
                        name: "FK_CategoryEntityPostEntity_Category_CategoriesId",
                        column: x => x.CategoriesId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CategoryEntityPostEntity_Post_PostEntityId",
                        column: x => x.PostEntityId,
                        principalTable: "Post",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CategoryEntityPostEntity_PostEntityId",
                table: "CategoryEntityPostEntity",
                column: "PostEntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CategoryEntityPostEntity");

            migrationBuilder.AddColumn<Guid>(
                name: "PostEntityId",
                table: "Category",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Category_PostEntityId",
                table: "Category",
                column: "PostEntityId");

            migrationBuilder.AddForeignKey(
                name: "FK_Category_Post_PostEntityId",
                table: "Category",
                column: "PostEntityId",
                principalTable: "Post",
                principalColumn: "Id");
        }
    }
}
