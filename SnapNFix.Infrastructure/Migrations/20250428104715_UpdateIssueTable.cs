using System;
using Microsoft.EntityFrameworkCore.Migrations;
using NetTopologySuite.Geometries;

#nullable disable

namespace SnapNFix.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIssueTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issue_SnapReport_MainReportId",
                table: "Issue");

            migrationBuilder.DropIndex(
                name: "IX_Issue_MainReportId",
                table: "Issue");

            migrationBuilder.DropColumn(
                name: "MainReportId",
                table: "Issue");

            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Issue",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "Issue",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Issue",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Point>(
                name: "Location",
                table: "Issue",
                type: "geography(Point,4326)",
                nullable: false);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Issue",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Issue_Category",
                table: "Issue",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_Issue_Location",
                table: "Issue",
                column: "Location")
                .Annotation("Npgsql:IndexMethod", "GIST");

            migrationBuilder.CreateIndex(
                name: "IX_Issue_Status",
                table: "Issue",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Issue_Category",
                table: "Issue");

            migrationBuilder.DropIndex(
                name: "IX_Issue_Location",
                table: "Issue");

            migrationBuilder.DropIndex(
                name: "IX_Issue_Status",
                table: "Issue");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Issue");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Issue");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Issue");

            migrationBuilder.DropColumn(
                name: "Location",
                table: "Issue");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Issue");

            migrationBuilder.AddColumn<Guid>(
                name: "MainReportId",
                table: "Issue",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Issue_MainReportId",
                table: "Issue",
                column: "MainReportId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Issue_SnapReport_MainReportId",
                table: "Issue",
                column: "MainReportId",
                principalTable: "SnapReport",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
