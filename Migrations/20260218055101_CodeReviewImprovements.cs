using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class CodeReviewImprovements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Legacy Clients table cleanup ──
            // The old Clients table was superseded by ClientProfiles but never dropped.
            // Drop orphaned FKs and columns referencing the old Clients table.

            // Drop FKs pointing to Clients table (safe - these are legacy references)
            migrationBuilder.Sql("PRAGMA foreign_keys = OFF;");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientBadges_Clients_ClientId",
                table: "ClientBadges");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Clients_ClientId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Clients_ClientId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PhotoShoots_Clients_ClientId",
                table: "PhotoShoots");

            // Drop indexes on legacy ClientId columns
            migrationBuilder.DropIndex(
                name: "IX_PhotoShoots_ClientId",
                table: "PhotoShoots");

            migrationBuilder.DropIndex(
                name: "IX_Invoices_ClientId",
                table: "Invoices");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ClientId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_ClientBadges_ClientId",
                table: "ClientBadges");

            // Drop legacy ClientId columns
            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "PhotoShoots");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "ClientBadges");

            // Drop the legacy Clients table
            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.Sql("PRAGMA foreign_keys = ON;");

            // ── FK behavior changes ──

            // ClientProfile: Cascade → Restrict
            migrationBuilder.DropForeignKey(
                name: "FK_ClientProfiles_AspNetUsers_UserId",
                table: "ClientProfiles");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientProfiles_AspNetUsers_UserId",
                table: "ClientProfiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Invoice→ClientProfile: SetNull → Restrict
            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_ClientProfiles_ClientProfileId",
                table: "Invoices");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_ClientProfiles_ClientProfileId",
                table: "Invoices",
                column: "ClientProfileId",
                principalTable: "ClientProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Contract→PhotoShoot: SetNull → Restrict
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_PhotoShoots_PhotoShootId",
                table: "Contracts");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_PhotoShoots_PhotoShootId",
                table: "Contracts",
                column: "PhotoShootId",
                principalTable: "PhotoShoots",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // ── New columns on Contract ──

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Contracts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedDate",
                table: "Contracts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExpirationDate",
                table: "Contracts",
                type: "TEXT",
                nullable: true);

            // ── New columns on Invoice ──

            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "Invoices",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentTransactionId",
                table: "Invoices",
                type: "TEXT",
                maxLength: 255,
                nullable: true);

            // ── New indexes ──

            migrationBuilder.CreateIndex(
                name: "IX_Contract_IsDeleted",
                table: "Contracts",
                column: "IsDeleted");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ── Remove new indexes ──
            migrationBuilder.DropIndex(
                name: "IX_Contract_IsDeleted",
                table: "Contracts");

            // ── Remove new Invoice columns ──
            migrationBuilder.DropColumn(
                name: "PaymentTransactionId",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "Invoices");

            // ── Remove new Contract columns ──
            migrationBuilder.DropColumn(
                name: "ExpirationDate",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "DeletedDate",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Contracts");

            // ── Revert FK behavior changes ──

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_PhotoShoots_PhotoShootId",
                table: "Contracts");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_PhotoShoots_PhotoShootId",
                table: "Contracts",
                column: "PhotoShootId",
                principalTable: "PhotoShoots",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_ClientProfiles_ClientProfileId",
                table: "Invoices");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_ClientProfiles_ClientProfileId",
                table: "Invoices",
                column: "ClientProfileId",
                principalTable: "ClientProfiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.DropForeignKey(
                name: "FK_ClientProfiles_AspNetUsers_UserId",
                table: "ClientProfiles");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientProfiles_AspNetUsers_UserId",
                table: "ClientProfiles",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
