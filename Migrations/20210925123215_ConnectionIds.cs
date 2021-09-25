using Microsoft.EntityFrameworkCore.Migrations;

namespace Photofy.Migrations
{
    public partial class ConnectionIds : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "ConnectionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ConnectionId",
                table: "Users",
                newName: "Id");
        }
    }
}
