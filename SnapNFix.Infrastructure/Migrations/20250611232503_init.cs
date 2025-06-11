using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SnapNFix.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:btree_gist", ",,")
                .Annotation("Npgsql:PostgresExtension:pg_stat_statements", ",,")
                .Annotation("Npgsql:PostgresExtension:pg_trgm", ",,")
                .Annotation("Npgsql:PostgresExtension:postgis", ",,")
                .Annotation("Npgsql:PostgresExtension:uuid-ossp", ",,");

            migrationBuilder.CreateTable(
                name: "Issues",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<Point>(type: "geometry (point, 4326)", nullable: false),
                    Road = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: ""),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: ""),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Egypt"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Status = table.Column<string>(type: "text", nullable: false),
                    Severity = table.Column<string>(type: "text", nullable: false, defaultValue: "Unspecified")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Issues", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FirstName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ImagePath = table.Column<string>(type: "text", nullable: false),
                    Username = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "NotSpecified"),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "date", nullable: false, defaultValueSql: "NOW()"),
                    UpdatedAt = table.Column<DateTime>(type: "date", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: false),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RoleClaims_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FastReport",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FastReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FastReport_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_FastReport_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SnapReports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IssueId = table.Column<Guid>(type: "uuid", nullable: true),
                    Comment = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, defaultValue: ""),
                    ImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Location = table.Column<Point>(type: "geometry (point, 4326)", nullable: false),
                    Road = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false, defaultValue: ""),
                    City = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: ""),
                    State = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: ""),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, defaultValue: "Egypt"),
                    ImageStatus = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    TaskId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Threshold = table.Column<double>(type: "double precision", precision: 5, scale: 4, nullable: true),
                    ReportCategory = table.Column<string>(type: "text", nullable: false, defaultValue: "NotSpecified"),
                    Severity = table.Column<string>(type: "text", nullable: false, defaultValue: "Unspecified"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SnapReports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SnapReports_Issues_IssueId",
                        column: x => x.IssueId,
                        principalTable: "Issues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_SnapReports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserClaims_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "UserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_UserLogins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_UserRoles_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserRoles_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_UserTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshToken",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Expires = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UserDeviceId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshToken", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshToken_UserDevice_UserDeviceId",
                        column: x => x.UserDeviceId,
                        principalTable: "UserDevice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FastReport_IssueId",
                table: "FastReport",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_FastReport_UserId",
                table: "FastReport",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_FastReport_UserId_IssueId",
                table: "FastReport",
                columns: new[] { "UserId", "IssueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Issues_Category",
                table: "Issues",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_Category_Status",
                table: "Issues",
                columns: new[] { "Category", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_City",
                table: "Issues",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_City_Status",
                table: "Issues",
                columns: new[] { "City", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_Country",
                table: "Issues",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_CreatedAt",
                table: "Issues",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_Location",
                table: "Issues",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_Severity",
                table: "Issues",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_State",
                table: "Issues",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_Status",
                table: "Issues",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_Status_CreatedAt",
                table: "Issues",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_Token",
                table: "RefreshToken",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefreshToken_UserDeviceId",
                table: "RefreshToken",
                column: "UserDeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RoleClaims_RoleId",
                table: "RoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "Roles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_City",
                table: "SnapReports",
                column: "City");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_City_ImageStatus",
                table: "SnapReports",
                columns: new[] { "City", "ImageStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_Country",
                table: "SnapReports",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_CreatedAt",
                table: "SnapReports",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_DeletedAt",
                table: "SnapReports",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_DeletedAt_CreatedAt",
                table: "SnapReports",
                columns: new[] { "DeletedAt", "CreatedAt" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_ImageStatus",
                table: "SnapReports",
                column: "ImageStatus");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_ImageStatus_CreatedAt",
                table: "SnapReports",
                columns: new[] { "ImageStatus", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_IssueId",
                table: "SnapReports",
                column: "IssueId");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_IssueId_CreatedAt",
                table: "SnapReports",
                columns: new[] { "IssueId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_Location",
                table: "SnapReports",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "gist");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_ReportCategory",
                table: "SnapReports",
                column: "ReportCategory");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_ReportCategory_ImageStatus",
                table: "SnapReports",
                columns: new[] { "ReportCategory", "ImageStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_Severity",
                table: "SnapReports",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_State",
                table: "SnapReports",
                column: "State");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_UserId",
                table: "SnapReports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_UserId_CreatedAt",
                table: "SnapReports",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_UserClaims_UserId",
                table: "UserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevice_DeviceName_DeviceId",
                table: "UserDevice",
                columns: new[] { "DeviceName", "DeviceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDevice_UserId",
                table: "UserDevice",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLogins_UserId",
                table: "UserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserRoles_RoleId",
                table: "UserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "Users",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_DeletedAt",
                table: "Users",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Users_FirstName_LastName",
                table: "Users",
                columns: new[] { "FirstName", "LastName" },
                filter: "\"DeletedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Gender",
                table: "Users",
                column: "Gender");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsDeleted",
                table: "Users",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Users_PhoneNumber",
                table: "Users",
                column: "PhoneNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "Users",
                column: "NormalizedUserName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FastReport");

            migrationBuilder.DropTable(
                name: "RefreshToken");

            migrationBuilder.DropTable(
                name: "RoleClaims");

            migrationBuilder.DropTable(
                name: "SnapReports");

            migrationBuilder.DropTable(
                name: "UserClaims");

            migrationBuilder.DropTable(
                name: "UserLogins");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserTokens");

            migrationBuilder.DropTable(
                name: "UserDevice");

            migrationBuilder.DropTable(
                name: "Issues");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
