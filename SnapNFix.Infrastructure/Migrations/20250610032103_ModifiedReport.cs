using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnapNFix.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModifiedReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SnapReport_ImageStatus",
                table: "SnapReport");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SnapReport_ImageStatus",
                table: "SnapReport",
                column: "ImageStatus");
        }
    }
}
