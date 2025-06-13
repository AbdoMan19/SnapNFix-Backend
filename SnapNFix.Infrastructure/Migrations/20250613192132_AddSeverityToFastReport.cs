using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnapNFix.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSeverityToFastReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "SnapReports",
                type: "text",
                nullable: false,
                defaultValue: "Low",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Unspecified");

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "Issues",
                type: "text",
                nullable: false,
                defaultValue: "NotSpecified",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Unspecified");

            migrationBuilder.AddColumn<string>(
                name: "Severity",
                table: "FastReport",
                type: "text",
                nullable: false,
                defaultValue: "Low");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Severity",
                table: "FastReport");

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "SnapReports",
                type: "text",
                nullable: false,
                defaultValue: "Unspecified",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Low");

            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "Issues",
                type: "text",
                nullable: false,
                defaultValue: "Unspecified",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "NotSpecified");
        }
    }
}
