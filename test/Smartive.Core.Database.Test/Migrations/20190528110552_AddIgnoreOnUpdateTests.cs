using Microsoft.EntityFrameworkCore.Migrations;

namespace Smartive.Core.Database.Test.Migrations
{
    public partial class AddIgnoreOnUpdateTests : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DontSetOnUpdate",
                table: "Users",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SetOnUpdate",
                table: "Users",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DontSetOnUpdate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SetOnUpdate",
                table: "Users");
        }
    }
}
