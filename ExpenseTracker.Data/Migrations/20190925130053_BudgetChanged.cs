using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpenseTracker.Data.Migrations
{
    public partial class BudgetChanged : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_IncomeCategories_IncomeCategoryId",
                table: "Budgets");

            migrationBuilder.RenameColumn(
                name: "IncomeCategoryId",
                table: "Budgets",
                newName: "ExpenseCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Budgets_IncomeCategoryId",
                table: "Budgets",
                newName: "IX_Budgets_ExpenseCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_ExpenseCategories_ExpenseCategoryId",
                table: "Budgets",
                column: "ExpenseCategoryId",
                principalTable: "ExpenseCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Budgets_ExpenseCategories_ExpenseCategoryId",
                table: "Budgets");

            migrationBuilder.RenameColumn(
                name: "ExpenseCategoryId",
                table: "Budgets",
                newName: "IncomeCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Budgets_ExpenseCategoryId",
                table: "Budgets",
                newName: "IX_Budgets_IncomeCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Budgets_IncomeCategories_IncomeCategoryId",
                table: "Budgets",
                column: "IncomeCategoryId",
                principalTable: "IncomeCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
