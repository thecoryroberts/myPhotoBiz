using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class AddPackageToPhotoShootAndInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuardianName",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "PackageName",
                table: "Contracts");

            migrationBuilder.AddColumn<int>(
                name: "BookingRequestId",
                table: "PhotoShoots",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServicePackageId",
                table: "PhotoShoots",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServicePackageId",
                table: "Invoices",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhotoShoots_BookingRequestId",
                table: "PhotoShoots",
                column: "BookingRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_PhotoShoots_ServicePackageId",
                table: "PhotoShoots",
                column: "ServicePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ServicePackageId",
                table: "Invoices",
                column: "ServicePackageId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_ServicePackages_ServicePackageId",
                table: "Invoices",
                column: "ServicePackageId",
                principalTable: "ServicePackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PhotoShoots_BookingRequests_BookingRequestId",
                table: "PhotoShoots",
                column: "BookingRequestId",
                principalTable: "BookingRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PhotoShoots_ServicePackages_ServicePackageId",
                table: "PhotoShoots",
                column: "ServicePackageId",
                principalTable: "ServicePackages",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_ServicePackages_ServicePackageId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PhotoShoots_BookingRequests_BookingRequestId",
                table: "PhotoShoots");

            migrationBuilder.DropForeignKey(
                name: "FK_PhotoShoots_ServicePackages_ServicePackageId",
                table: "PhotoShoots");

            migrationBuilder.DropIndex(
                name: "IX_PhotoShoots_BookingRequestId",
                table: "PhotoShoots");

            migrationBuilder.DropIndex(
                name: "IX_PhotoShoots_ServicePackageId",
                table: "PhotoShoots");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_ServicePackageId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "BookingRequestId",
                table: "PhotoShoots");

            migrationBuilder.DropColumn(
                name: "ServicePackageId",
                table: "PhotoShoots");

            migrationBuilder.DropColumn(
                name: "ServicePackageId",
                table: "Invoices");

            migrationBuilder.AddColumn<string>(
                name: "GuardianName",
                table: "Contracts",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PackageName",
                table: "Contracts",
                type: "TEXT",
                maxLength: 200,
                nullable: true);
        }
    }
}
