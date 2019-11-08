using Microsoft.EntityFrameworkCore.Migrations;

namespace ExpenseTracker.Data.Migrations
{
    public partial class addOauthProp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountNumber",
                table: "GoogleAuths",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountNumber",
                table: "GoogleAuths");
        }
    }
}
