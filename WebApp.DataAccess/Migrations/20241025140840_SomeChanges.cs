using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.DataAccess.Migrations
{
    public partial class SomeChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
#pragma warning disable CA1062 // Validate arguments of public methods
            _ = migrationBuilder.DropForeignKey(
                name: "FK_PostReplies_AspNetUsers_UserId",
                table: "PostReplies");
#pragma warning restore CA1062 // Validate arguments of public methods

            _ = migrationBuilder.DropForeignKey(
                name: "FK_PostReplies_Posts_PostId",
                table: "PostReplies");

            _ = migrationBuilder.DropIndex(
                name: "IX_PostReplies_UserId",
                table: "PostReplies");

            _ = migrationBuilder.DropColumn(
                name: "Image",
                table: "Threads");

            _ = migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "PostReplies",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            _ = migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "PostReplies",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            _ = migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "PostReplies",
                type: "nvarchar(450)",
                nullable: true);

            _ = migrationBuilder.CreateIndex(
                name: "IX_PostReplies_UserId1",
                table: "PostReplies",
                column: "UserId1");

            _ = migrationBuilder.AddForeignKey(
                name: "FK_PostReplies_AspNetUsers_UserId1",
                table: "PostReplies",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            _ = migrationBuilder.AddForeignKey(
                name: "FK_PostReplies_Posts_PostId",
                table: "PostReplies",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
#pragma warning disable CA1062 // Validate arguments of public methods
            _ = migrationBuilder.DropForeignKey(
                name: "FK_PostReplies_AspNetUsers_UserId1",
                table: "PostReplies");
#pragma warning restore CA1062 // Validate arguments of public methods

            _ = migrationBuilder.DropForeignKey(
                name: "FK_PostReplies_Posts_PostId",
                table: "PostReplies");

            _ = migrationBuilder.DropIndex(
                name: "IX_PostReplies_UserId1",
                table: "PostReplies");

            _ = migrationBuilder.DropColumn(
                name: "UserId1",
                table: "PostReplies");

            _ = migrationBuilder.AddColumn<string>(
                name: "Image",
                table: "Threads",
                type: "nvarchar(max)",
                nullable: true);

            _ = migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "PostReplies",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            _ = migrationBuilder.AlterColumn<int>(
                name: "PostId",
                table: "PostReplies",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            _ = migrationBuilder.CreateIndex(
                name: "IX_PostReplies_UserId",
                table: "PostReplies",
                column: "UserId");

            _ = migrationBuilder.AddForeignKey(
                name: "FK_PostReplies_AspNetUsers_UserId",
                table: "PostReplies",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            _ = migrationBuilder.AddForeignKey(
                name: "FK_PostReplies_Posts_PostId",
                table: "PostReplies",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id");
        }
    }
}
