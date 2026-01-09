using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class Categotry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "ClientProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ContactPreference",
                table: "ClientProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "ClientProfiles",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ClientProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ReferralDetails",
                table: "ClientProfiles",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReferralSource",
                table: "ClientProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ClientProfiles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BookingRequests_ClientProfiles_ClientProfileId1",
                table: "BookingRequests");

            migrationBuilder.DropIndex(
                name: "IX_BookingRequests_ClientProfileId1",
                table: "BookingRequests");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "ClientProfiles");

            migrationBuilder.DropColumn(
                name: "ContactPreference",
                table: "ClientProfiles");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "ClientProfiles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ClientProfiles");

            migrationBuilder.DropColumn(
                name: "ReferralDetails",
                table: "ClientProfiles");

            migrationBuilder.DropColumn(
                name: "ReferralSource",
                table: "ClientProfiles");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ClientProfiles");

            migrationBuilder.DropColumn(
                name: "ClientProfileId1",
                table: "BookingRequests");
        }
    }
}
