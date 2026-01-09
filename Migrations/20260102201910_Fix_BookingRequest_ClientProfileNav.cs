using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class Fix_BookingRequest_ClientProfileNav : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingRequests_ClientProfiles_ClientProfileId1",
                table: "BookingRequests");

            migrationBuilder.DropIndex(
                name: "IX_BookingRequests_ClientProfileId1",
                table: "BookingRequests");

            migrationBuilder.DropColumn(
                name: "ClientProfileId1",
                table: "BookingRequests");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientProfileId1",
                table: "BookingRequests",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BookingRequests_ClientProfileId1",
                table: "BookingRequests",
                column: "ClientProfileId1");

            migrationBuilder.AddForeignKey(
                name: "FK_BookingRequests_ClientProfiles_ClientProfileId1",
                table: "BookingRequests",
                column: "ClientProfileId1",
                principalTable: "ClientProfiles",
                principalColumn: "Id");
        }
    }
}
