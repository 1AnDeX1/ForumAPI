using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.DataAccess.Migrations
{
    public partial class DeleteUserNameProperty : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
#pragma warning disable CA1062 // Validate arguments of public methods
            _ = migrationBuilder.DropColumn(
                name: "Name",
                table: "AspNetUsers");
#pragma warning restore CA1062 // Validate arguments of public methods
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
#pragma warning disable CA1062 // Validate arguments of public methods
            _ = migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
#pragma warning restore CA1062 // Validate arguments of public methods
        }
    }
}
