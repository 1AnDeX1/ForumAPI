using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.DataAccess.Migrations
{
    public partial class RedoOfThread : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
#pragma warning disable IDE0058 // Expression value is never used
#pragma warning disable CA1062 // Validate arguments of public methods
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Threads",
                newName: "Content");
#pragma warning restore CA1062 // Validate arguments of public methods
#pragma warning restore IDE0058 // Expression value is never used

#pragma warning disable IDE0058 // Expression value is never used
            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "Threads",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
#pragma warning restore IDE0058 // Expression value is never used
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
#pragma warning disable CA1062 // Validate arguments of public methods
#pragma warning disable IDE0058 // Expression value is never used
            migrationBuilder.RenameColumn(
                name: "Content",
                table: "Threads",
                newName: "Description");
#pragma warning restore IDE0058 // Expression value is never used
#pragma warning restore CA1062 // Validate arguments of public methods

#pragma warning disable IDE0058 // Expression value is never used
            migrationBuilder.AlterColumn<DateTime>(
                name: "Created",
                table: "Threads",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");
#pragma warning restore IDE0058 // Expression value is never used
        }
    }
}
