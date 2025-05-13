using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnapNFix.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addTaskIdToSnapReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Threshold",
                table: "SnapReport",
                type: "double precision",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SnapReport_TaskId",
                table: "SnapReport",
                column: "TaskId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SnapReport_TaskId",
                table: "SnapReport");

            migrationBuilder.DropColumn(
                name: "Threshold",
                table: "SnapReport");
        }
    }
}
