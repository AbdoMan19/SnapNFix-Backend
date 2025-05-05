using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnapNFix.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDeviceEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_Users_UserId",
                table: "RefreshToken");

            migrationBuilder.DropColumn(
                name: "RefreshTokenId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "RefreshToken",
                newName: "UserDeviceId");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshToken_UserId",
                table: "RefreshToken",
                newName: "IX_RefreshToken_UserDeviceId");

            migrationBuilder.CreateTable(
                name: "UserDevice",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Platform = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeviceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RefreshTokenId = table.Column<Guid>(type: "uuid", nullable: true),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDevice", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDevice_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserDevice_DeviceName_DeviceId",
                table: "UserDevice",
                columns: new[] { "DeviceName", "DeviceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDevice_UserId",
                table: "UserDevice",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshToken_UserDevice_UserDeviceId",
                table: "RefreshToken",
                column: "UserDeviceId",
                principalTable: "UserDevice",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_UserDevice_UserDeviceId",
                table: "RefreshToken");

            migrationBuilder.DropTable(
                name: "UserDevice");

            migrationBuilder.RenameColumn(
                name: "UserDeviceId",
                table: "RefreshToken",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshToken_UserDeviceId",
                table: "RefreshToken",
                newName: "IX_RefreshToken_UserId");

            migrationBuilder.AddColumn<Guid>(
                name: "RefreshTokenId",
                table: "Users",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshToken_Users_UserId",
                table: "RefreshToken",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
