using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpenseTracker.Data.Migrations
{
    public partial class adminCatAdd2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppUserId",
                table: "AdminExpenseCategories",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminExpenseCategories_AppUserId",
                table: "AdminExpenseCategories",
                column: "AppUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdminExpenseCategories_AspNetUsers_AppUserId",
                table: "AdminExpenseCategories",
                column: "AppUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdminExpenseCategories_AspNetUsers_AppUserId",
                table: "AdminExpenseCategories");

            migrationBuilder.DropIndex(
                name: "IX_AdminExpenseCategories_AppUserId",
                table: "AdminExpenseCategories");

            migrationBuilder.DropColumn(
                name: "AppUserId",
                table: "AdminExpenseCategories");
        }
    }
}
