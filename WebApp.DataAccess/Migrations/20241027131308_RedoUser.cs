using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApp.DataAccess.Migrations
{
    public partial class RedoUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
#pragma warning disable CA1062 // Validate arguments of public methods
            _ = migrationBuilder.DropForeignKey(
                name: "FK_PostReplies_ApplicationUsers_UserId1",
                table: "PostReplies");
#pragma warning restore CA1062 // Validate arguments of public methods

            _ = migrationBuilder.DropForeignKey(name: "FK_Posts_ApplicationUsers_UserId1", table: "Posts");

            _ = migrationBuilder.DropPrimaryKey(name: "PK_ApplicationUsers", table: "ApplicationUsers");

            _ = migrationBuilder.DropIndex(name: "IX_ApplicationUsers_Email", table: "ApplicationUsers");

            _ = migrationBuilder.DropIndex(name: "IX_ApplicationUsers_Username", table: "ApplicationUsers");

            _ = migrationBuilder.RenameTable(
                name: "ApplicationUsers",
                newName: "AspNetUsers");

            _ = migrationBuilder.RenameColumn(name: "Username", table: "AspNetUsers", newName: "UserName");

            _ = migrationBuilder.AlterColumn<string>(name: "UserId1", table: "Posts", type: "nvarchar(450)", nullable: true, oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);

            _ = migrationBuilder.AlterColumn<string>(name: "UserId1", table: "PostReplies", type: "nvarchar(450)", nullable: true, oldClrType: typeof(Guid), oldType: "uniqueidentifier", oldNullable: true);

            _ = migrationBuilder.AlterColumn<string>(name: "UserName", table: "AspNetUsers", type: "nvarchar(256)", maxLength: 256, nullable: true, oldClrType: typeof(string), oldType: "nvarchar(450)", oldNullable: true);

            _ = migrationBuilder.AlterColumn<string>(name: "Email", table: "AspNetUsers", type: "nvarchar(256)", maxLength: 256, nullable: true, oldClrType: typeof(string), oldType: "nvarchar(450)", oldNullable: true);

            _ = migrationBuilder.AlterColumn<string>(name: "Id", table: "AspNetUsers", type: "nvarchar(450)", nullable: false, oldClrType: typeof(Guid), oldType: "uniqueidentifier");

            _ = migrationBuilder.AddColumn<int>(name: "AccessFailedCount", table: "AspNetUsers", type: "int", nullable: false, defaultValue: 0);

            _ = migrationBuilder.AddColumn<string>(name: "ConcurrencyStamp", table: "AspNetUsers", type: "nvarchar(max)", nullable: true);

            _ = migrationBuilder.AddColumn<bool>(name: "EmailConfirmed", table: "AspNetUsers", type: "bit", nullable: false, defaultValue: false);

            _ = migrationBuilder.AddColumn<bool>(name: "LockoutEnabled", table: "AspNetUsers", type: "bit", nullable: false, defaultValue: false);

            _ = migrationBuilder.AddColumn<DateTimeOffset>(name: "LockoutEnd", table: "AspNetUsers", type: "datetimeoffset", nullable: true);

            _ = migrationBuilder.AddColumn<string>(name: "Name", table: "AspNetUsers", type: "nvarchar(max)", nullable: true);

            _ = migrationBuilder.AddColumn<string>(name: "NormalizedEmail", table: "AspNetUsers", type: "nvarchar(256)", maxLength: 256, nullable: true);

            _ = migrationBuilder.AddColumn<string>(name: "NormalizedUserName", table: "AspNetUsers", type: "nvarchar(256)", maxLength: 256, nullable: true);

            _ = migrationBuilder.AddColumn<string>(name: "PhoneNumber", table: "AspNetUsers", type: "nvarchar(max)", nullable: true);

            _ = migrationBuilder.AddColumn<bool>(name: "PhoneNumberConfirmed", table: "AspNetUsers", type: "bit", nullable: false, defaultValue: false);

            _ = migrationBuilder.AddColumn<string>(name: "SecurityStamp", table: "AspNetUsers", type: "nvarchar(max)", nullable: true);

            _ = migrationBuilder.AddColumn<bool>(name: "TwoFactorEnabled", table: "AspNetUsers", type: "bit", nullable: false, defaultValue: false);

            _ = migrationBuilder.AddPrimaryKey(name: "PK_AspNetUsers", table: "AspNetUsers", column: "Id");

            _ = migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            _ = migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    _ = table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    _ = table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    _ = table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                },
                constraints: table =>
                {
                    _ = table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    _ = table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    _ = table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            _ = migrationBuilder.CreateIndex(name: "EmailIndex", table: "AspNetUsers", column: "NormalizedEmail");

            _ = migrationBuilder.CreateIndex(name: "UserNameIndex", table: "AspNetUsers", column: "NormalizedUserName", unique: true, filter: "[NormalizedUserName] IS NOT NULL");

            _ = migrationBuilder.CreateIndex(name: "IX_AspNetRoleClaims_RoleId", table: "AspNetRoleClaims", column: "RoleId");

            _ = migrationBuilder.CreateIndex(name: "RoleNameIndex", table: "AspNetRoles", column: "NormalizedName", unique: true, filter: "[NormalizedName] IS NOT NULL");

            _ = migrationBuilder.CreateIndex(name: "IX_AspNetUserClaims_UserId", table: "AspNetUserClaims", column: "UserId");

            _ = migrationBuilder.CreateIndex(name: "IX_AspNetUserLogins_UserId", table: "AspNetUserLogins", column: "UserId");

            _ = migrationBuilder.CreateIndex(name: "IX_AspNetUserRoles_RoleId", table: "AspNetUserRoles", column: "RoleId");

            _ = migrationBuilder.AddForeignKey(name: "FK_PostReplies_AspNetUsers_UserId1", table: "PostReplies", column: "UserId1", principalTable: "AspNetUsers", principalColumn: "Id");

            _ = migrationBuilder.AddForeignKey(name: "FK_Posts_AspNetUsers_UserId1", table: "Posts", column: "UserId1", principalTable: "AspNetUsers", principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
#pragma warning disable CA1062 // Validate arguments of public methods
            _ = migrationBuilder.DropForeignKey(name: "FK_PostReplies_AspNetUsers_UserId1", table: "PostReplies");
#pragma warning restore CA1062 // Validate arguments of public methods

            _ = migrationBuilder.DropForeignKey(name: "FK_Posts_AspNetUsers_UserId1", table: "Posts");

            _ = migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            _ = migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            _ = migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            _ = migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            _ = migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            _ = migrationBuilder.DropTable(
                name: "AspNetRoles");

            _ = migrationBuilder.DropPrimaryKey(name: "PK_AspNetUsers", table: "AspNetUsers");

            _ = migrationBuilder.DropIndex(name: "EmailIndex", table: "AspNetUsers");

            _ = migrationBuilder.DropIndex(name: "UserNameIndex", table: "AspNetUsers");

            _ = migrationBuilder.DropColumn(name: "AccessFailedCount", table: "AspNetUsers");

            _ = migrationBuilder.DropColumn(name: "ConcurrencyStamp", table: "AspNetUsers");

            _ = migrationBuilder.DropColumn(name: "EmailConfirmed", table: "AspNetUsers");

            _ = migrationBuilder.DropColumn(name: "LockoutEnabled", table: "AspNetUsers");

            _ = migrationBuilder.DropColumn(name: "LockoutEnd", table: "AspNetUsers");

            _ = migrationBuilder.DropColumn(name: "Name", table: "AspNetUsers");

            _ = migrationBuilder.DropColumn(name: "NormalizedEmail", table: "AspNetUsers");

            _ = migrationBuilder.DropColumn(name: "NormalizedUserName", table: "AspNetUsers");

            _ = migrationBuilder.DropColumn(name: "PhoneNumber", table: "AspNetUsers");

            _ = migrationBuilder.DropColumn(name: "PhoneNumberConfirmed", table: "AspNetUsers");

            _ = migrationBuilder.DropColumn(name: "SecurityStamp", table: "AspNetUsers");

            _ = migrationBuilder.DropColumn(name: "TwoFactorEnabled", table: "AspNetUsers");

            _ = migrationBuilder.RenameTable(
                name: "AspNetUsers",
                newName: "ApplicationUsers");

            _ = migrationBuilder.RenameColumn(name: "UserName", table: "ApplicationUsers", newName: "Username");

            _ = migrationBuilder.AlterColumn<Guid>(name: "UserId1", table: "Posts", type: "uniqueidentifier", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(450)", oldNullable: true);

            _ = migrationBuilder.AlterColumn<Guid>(name: "UserId1", table: "PostReplies", type: "uniqueidentifier", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(450)", oldNullable: true);

            _ = migrationBuilder.AlterColumn<string>(name: "Username", table: "ApplicationUsers", type: "nvarchar(450)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(256)", oldMaxLength: 256, oldNullable: true);

            _ = migrationBuilder.AlterColumn<string>(name: "Email", table: "ApplicationUsers", type: "nvarchar(450)", nullable: true, oldClrType: typeof(string), oldType: "nvarchar(256)", oldMaxLength: 256, oldNullable: true);

            _ = migrationBuilder.AlterColumn<Guid>(name: "Id", table: "ApplicationUsers", type: "uniqueidentifier", nullable: false, oldClrType: typeof(string), oldType: "nvarchar(450)");

            _ = migrationBuilder.AddPrimaryKey(name: "PK_ApplicationUsers", table: "ApplicationUsers", column: "Id");

            _ = migrationBuilder.CreateIndex(name: "IX_ApplicationUsers_Email", table: "ApplicationUsers", column: "Email", unique: true, filter: "[Email] IS NOT NULL");

            _ = migrationBuilder.CreateIndex(name: "IX_ApplicationUsers_Username", table: "ApplicationUsers", column: "Username", unique: true, filter: "[Username] IS NOT NULL");

            _ = migrationBuilder.AddForeignKey(name: "FK_PostReplies_ApplicationUsers_UserId1", table: "PostReplies", column: "UserId1", principalTable: "ApplicationUsers", principalColumn: "Id");

            _ = migrationBuilder.AddForeignKey(name: "FK_Posts_ApplicationUsers_UserId1", table: "Posts", column: "UserId1", principalTable: "ApplicationUsers", principalColumn: "Id");
        }
    }
}
