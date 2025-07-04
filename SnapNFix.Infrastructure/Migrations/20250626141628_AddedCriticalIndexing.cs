using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SnapNFix.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedCriticalIndexing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FastReport_Issues_IssueId",
                table: "FastReport");

            migrationBuilder.DropForeignKey(
                name: "FK_FastReport_Users_UserId",
                table: "FastReport");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_UserDevice_UserDeviceId",
                table: "RefreshToken");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCitySubscription_CityChannel_CityChannelId",
                table: "UserCitySubscription");

            migrationBuilder.DropForeignKey(
                name: "FK_UserDevice_Users_UserId",
                table: "UserDevice");

            migrationBuilder.DropIndex(
                name: "IX_Users_CreatedAt",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_DeletedAt",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_FirstName_LastName",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Gender",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_City",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_City_ImageStatus",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_Country",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_DeletedAt",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_DeletedAt_CreatedAt",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_ImageStatus_CreatedAt",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_IssueId",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_IssueId_CreatedAt",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_ReportCategory",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_ReportCategory_ImageStatus",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_Severity",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_State",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_UserId_CreatedAt",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_UserId_IssueId",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_Issues_Category",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_City",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_City_Status",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_Country",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_Severity",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_State",
                table: "Issues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDevice",
                table: "UserDevice");

            migrationBuilder.DropIndex(
                name: "IX_UserDevice_DeviceName_DeviceId",
                table: "UserDevice");

            migrationBuilder.DropIndex(
                name: "IX_UserDevice_UserId",
                table: "UserDevice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FastReport",
                table: "FastReport");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CityChannel",
                table: "CityChannel");

            migrationBuilder.DropIndex(
                name: "IX_CityChannel_Name",
                table: "CityChannel");

            migrationBuilder.RenameTable(
                name: "UserDevice",
                newName: "UserDevices");

            migrationBuilder.RenameTable(
                name: "FastReport",
                newName: "FastReports");

            migrationBuilder.RenameTable(
                name: "CityChannel",
                newName: "CityChannels");

            migrationBuilder.RenameIndex(
                name: "IX_Issues_Status_CreatedAt",
                table: "Issues",
                newName: "IX_Issues_Status_Created");

            migrationBuilder.RenameIndex(
                name: "IX_FastReport_UserId_IssueId",
                table: "FastReports",
                newName: "IX_FastReports_User_Issue_Unique");

            migrationBuilder.RenameIndex(
                name: "IX_FastReport_UserId",
                table: "FastReports",
                newName: "IX_FastReports_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_FastReport_IssueId",
                table: "FastReports",
                newName: "IX_FastReports_IssueId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDevices",
                table: "UserDevices",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FastReports",
                table: "FastReports",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CityChannels",
                table: "CityChannels",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                filter: "\"Email\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsDeleted_Created",
                table: "Users",
                columns: new[] { "IsDeleted", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Name",
                table: "Users",
                columns: new[] { "FirstName", "LastName" },
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_City_Status",
                table: "SnapReports",
                columns: new[] { "City", "ImageStatus" },
                filter: "\"City\" IS NOT NULL AND \"City\" != ''");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_Issue_Created",
                table: "SnapReports",
                columns: new[] { "IssueId", "CreatedAt" },
                filter: "\"IssueId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_IssueId",
                table: "SnapReports",
                column: "IssueId",
                filter: "\"IssueId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_TaskId",
                table: "SnapReports",
                column: "TaskId",
                unique: true,
                filter: "\"TaskId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_User_Issue_Unique",
                table: "SnapReports",
                columns: new[] { "UserId", "IssueId" },
                unique: true,
                filter: "\"IssueId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_User_Status_Created",
                table: "SnapReports",
                columns: new[] { "UserId", "ImageStatus", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_City_State_Status",
                table: "Issues",
                columns: new[] { "City", "State", "Status" },
                filter: "\"City\" IS NOT NULL AND \"State\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_Severity_Status",
                table: "Issues",
                columns: new[] { "Severity", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Issues_State_Status",
                table: "Issues",
                columns: new[] { "State", "Status" },
                filter: "\"State\" IS NOT NULL AND \"State\" != ''");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_DeviceId",
                table: "UserDevices",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_FCMToken",
                table: "UserDevices",
                column: "FCMToken",
                filter: "\"FCMToken\" IS NOT NULL AND \"FCMToken\" != ''");

            migrationBuilder.CreateIndex(
                name: "IX_UserDevices_User_LastUsed",
                table: "UserDevices",
                columns: new[] { "UserId", "LastUsedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FastReports_Issue_Created",
                table: "FastReports",
                columns: new[] { "IssueId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FastReports_User_Created",
                table: "FastReports",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CityChannels_Active_Name",
                table: "CityChannels",
                columns: new[] { "IsActive", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_CityChannels_IsActive",
                table: "CityChannels",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_CityChannels_Name_State_Unique",
                table: "CityChannels",
                columns: new[] { "Name", "State" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FastReports_Issues_IssueId",
                table: "FastReports",
                column: "IssueId",
                principalTable: "Issues",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FastReports_Users_UserId",
                table: "FastReports",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshToken_UserDevices_UserDeviceId",
                table: "RefreshToken",
                column: "UserDeviceId",
                principalTable: "UserDevices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCitySubscription_CityChannels_CityChannelId",
                table: "UserCitySubscription",
                column: "CityChannelId",
                principalTable: "CityChannels",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserDevices_Users_UserId",
                table: "UserDevices",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FastReports_Issues_IssueId",
                table: "FastReports");

            migrationBuilder.DropForeignKey(
                name: "FK_FastReports_Users_UserId",
                table: "FastReports");

            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_UserDevices_UserDeviceId",
                table: "RefreshToken");

            migrationBuilder.DropForeignKey(
                name: "FK_UserCitySubscription_CityChannels_CityChannelId",
                table: "UserCitySubscription");

            migrationBuilder.DropForeignKey(
                name: "FK_UserDevices_Users_UserId",
                table: "UserDevices");

            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_IsDeleted_Created",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Name",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_City_Status",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_Issue_Created",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_IssueId",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_TaskId",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_User_Issue_Unique",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_SnapReports_User_Status_Created",
                table: "SnapReports");

            migrationBuilder.DropIndex(
                name: "IX_Issues_City_State_Status",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_Severity_Status",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_State_Status",
                table: "Issues");

            migrationBuilder.DropPrimaryKey(
                name: "PK_UserDevices",
                table: "UserDevices");

            migrationBuilder.DropIndex(
                name: "IX_UserDevices_DeviceId",
                table: "UserDevices");

            migrationBuilder.DropIndex(
                name: "IX_UserDevices_FCMToken",
                table: "UserDevices");

            migrationBuilder.DropIndex(
                name: "IX_UserDevices_User_LastUsed",
                table: "UserDevices");

            migrationBuilder.DropPrimaryKey(
                name: "PK_FastReports",
                table: "FastReports");

            migrationBuilder.DropIndex(
                name: "IX_FastReports_Issue_Created",
                table: "FastReports");

            migrationBuilder.DropIndex(
                name: "IX_FastReports_User_Created",
                table: "FastReports");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CityChannels",
                table: "CityChannels");

            migrationBuilder.DropIndex(
                name: "IX_CityChannels_Active_Name",
                table: "CityChannels");

            migrationBuilder.DropIndex(
                name: "IX_CityChannels_IsActive",
                table: "CityChannels");

            migrationBuilder.DropIndex(
                name: "IX_CityChannels_Name_State_Unique",
                table: "CityChannels");

            migrationBuilder.RenameTable(
                name: "UserDevices",
                newName: "UserDevice");

            migrationBuilder.RenameTable(
                name: "FastReports",
                newName: "FastReport");

            migrationBuilder.RenameTable(
                name: "CityChannels",
                newName: "CityChannel");

            migrationBuilder.RenameIndex(
                name: "IX_Issues_Status_Created",
                table: "Issues",
                newName: "IX_Issues_Status_CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_FastReports_UserId",
                table: "FastReport",
                newName: "IX_FastReport_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_FastReports_User_Issue_Unique",
                table: "FastReport",
                newName: "IX_FastReport_UserId_IssueId");

            migrationBuilder.RenameIndex(
                name: "IX_FastReports_IssueId",
                table: "FastReport",
                newName: "IX_FastReport_IssueId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_UserDevice",
                table: "UserDevice",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_FastReport",
                table: "FastReport",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CityChannel",
                table: "CityChannel",
                column: "Id");

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
                name: "IX_SnapReports_DeletedAt",
                table: "SnapReports",
                column: "DeletedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_DeletedAt_CreatedAt",
                table: "SnapReports",
                columns: new[] { "DeletedAt", "CreatedAt" },
                filter: "\"DeletedAt\" IS NULL");

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
                name: "IX_SnapReports_UserId_CreatedAt",
                table: "SnapReports",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SnapReports_UserId_IssueId",
                table: "SnapReports",
                columns: new[] { "UserId", "IssueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Issues_Category",
                table: "Issues",
                column: "Category");

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
                name: "IX_Issues_Severity",
                table: "Issues",
                column: "Severity");

            migrationBuilder.CreateIndex(
                name: "IX_Issues_State",
                table: "Issues",
                column: "State");

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
                name: "IX_CityChannel_Name",
                table: "CityChannel",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FastReport_Issues_IssueId",
                table: "FastReport",
                column: "IssueId",
                principalTable: "Issues",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_FastReport_Users_UserId",
                table: "FastReport",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshToken_UserDevice_UserDeviceId",
                table: "RefreshToken",
                column: "UserDeviceId",
                principalTable: "UserDevice",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCitySubscription_CityChannel_CityChannelId",
                table: "UserCitySubscription",
                column: "CityChannelId",
                principalTable: "CityChannel",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserDevice_Users_UserId",
                table: "UserDevice",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
