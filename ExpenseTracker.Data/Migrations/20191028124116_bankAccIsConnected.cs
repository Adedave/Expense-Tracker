using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpenseTracker.Data.Migrations
{
    public partial class bankAccIsConnected : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GoogleAuthId",
                table: "BankAccounts",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConnnected",
                table: "BankAccounts",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_BankAccounts_GoogleAuthId",
                table: "BankAccounts",
                column: "GoogleAuthId");

            migrationBuilder.AddForeignKey(
                name: "FK_BankAccounts_GoogleAuths_GoogleAuthId",
                table: "BankAccounts",
                column: "GoogleAuthId",
                principalTable: "GoogleAuths",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BankAccounts_GoogleAuths_GoogleAuthId",
                table: "BankAccounts");

            migrationBuilder.DropIndex(
                name: "IX_BankAccounts_GoogleAuthId",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "GoogleAuthId",
                table: "BankAccounts");

            migrationBuilder.DropColumn(
                name: "IsConnnected",
                table: "BankAccounts");
        }
    }
}
