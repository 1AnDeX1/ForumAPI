using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.DataAccess.Migrations
{
    public partial class ChangeOfPostEntity : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
#pragma warning disable CA1062 // Validate arguments of public methods
#pragma warning disable IDE0058 // Expression value is never used
            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Posts",
                newName: "Created");
#pragma warning restore IDE0058 // Expression value is never used
#pragma warning restore CA1062 // Validate arguments of public methods
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
#pragma warning disable CA1062 // Validate arguments of public methods
#pragma warning disable IDE0058 // Expression value is never used
            migrationBuilder.RenameColumn(
                name: "Created",
                table: "Posts",
                newName: "CreatedAt");
#pragma warning restore IDE0058 // Expression value is never used
#pragma warning restore CA1062 // Validate arguments of public methods
        }
    }
}
