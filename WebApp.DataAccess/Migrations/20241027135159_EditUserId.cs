using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.DataAccess.Migrations
{
    public partial class EditUserId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
#pragma warning disable CA1062 // Validate arguments of public methods
            _ = migrationBuilder.DropForeignKey(
                name: "FK_PostReplies_AspNetUsers_UserId1",
                table: "PostReplies");
#pragma warning restore CA1062 // Validate arguments of public methods

            _ = migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_UserId1",
                table: "Posts");

            _ = migrationBuilder.DropIndex(
                name: "IX_Posts_UserId1",
                table: "Posts");

            _ = migrationBuilder.DropIndex(
                name: "IX_PostReplies_UserId1",
                table: "PostReplies");

            _ = migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Posts");

            _ = migrationBuilder.DropColumn(
                name: "UserId1",
                table: "PostReplies");

            _ = migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Threads",
                type: "nvarchar(450)",
                nullable: true);

            _ = migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Posts",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            _ = migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "PostReplies",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Threads_UserId",
                table: "Threads",
                column: "UserId");

            _ = migrationBuilder.CreateIndex(
                name: "IX_Posts_UserId",
                table: "Posts",
                column: "UserId");

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
                name: "FK_Posts_AspNetUsers_UserId",
                table: "Posts",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            _ = migrationBuilder.AddForeignKey(
                name: "FK_Threads_AspNetUsers_UserId",
                table: "Threads",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
#pragma warning disable CA1062 // Validate arguments of public methods
            _ = migrationBuilder.DropForeignKey(
                name: "FK_PostReplies_AspNetUsers_UserId",
                table: "PostReplies");
#pragma warning restore CA1062 // Validate arguments of public methods

            _ = migrationBuilder.DropForeignKey(
                name: "FK_Posts_AspNetUsers_UserId",
                table: "Posts");

            _ = migrationBuilder.DropForeignKey(
                name: "FK_Threads_AspNetUsers_UserId",
                table: "Threads");

            _ = migrationBuilder.DropIndex(
                name: "IX_Threads_UserId",
                table: "Threads");

            _ = migrationBuilder.DropIndex(
                name: "IX_Posts_UserId",
                table: "Posts");

            _ = migrationBuilder.DropIndex(
                name: "IX_PostReplies_UserId",
                table: "PostReplies");

            _ = migrationBuilder.DropColumn(
                name: "UserId",
                table: "Threads");

            _ = migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            _ = migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "Posts",
                type: "nvarchar(450)",
                nullable: true);

            _ = migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "PostReplies",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            _ = migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "PostReplies",
                type: "nvarchar(450)",
                nullable: true);

            _ = migrationBuilder.CreateIndex(
                name: "IX_Posts_UserId1",
                table: "Posts",
                column: "UserId1");

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
                name: "FK_Posts_AspNetUsers_UserId1",
                table: "Posts",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
