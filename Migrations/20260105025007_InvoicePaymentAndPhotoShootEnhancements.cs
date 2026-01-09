using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class InvoicePaymentAndPhotoShootEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhotoShoots_AspNetUsers_PhotographerId",
                table: "PhotoShoots");

            migrationBuilder.RenameColumn(
                name: "PhotographerId",
                table: "PhotoShoots",
                newName: "UpdatedByUserId");

            migrationBuilder.RenameIndex(
                name: "IX_PhotoShoots_PhotographerId",
                table: "PhotoShoots",
                newName: "IX_PhotoShoots_UpdatedByUserId");

            migrationBuilder.AddColumn<string>(
                name: "CreatedByUserId",
                table: "PhotoShoots",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "PhotoShoots",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "PhotoShoots",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextRecurrenceDate",
                table: "PhotoShoots",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecurrencePattern",
                table: "PhotoShoots",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShootType",
                table: "PhotoShoots",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                table: "Invoices",
                type: "TEXT",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "DepositAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "DepositPaid",
                table: "Invoices",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DepositPaidDate",
                table: "Invoices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Invoices",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRecurring",
                table: "Invoices",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextRecurrenceDate",
                table: "Invoices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecurrencePattern",
                table: "Invoices",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RefundAmount",
                table: "Invoices",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReminderSentDate",
                table: "Invoices",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Amount = table.Column<double>(type: "decimal(18,2)", nullable: false),
                    PaymentDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    PaymentMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    TransactionId = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsRefund = table.Column<bool>(type: "INTEGER", nullable: false),
                    RefundReason = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    InvoiceId = table.Column<int>(type: "INTEGER", nullable: false),
                    ProcessedByUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_AspNetUsers_ProcessedByUserId",
                        column: x => x.ProcessedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Payments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhotoShoot_IsDeleted",
                table: "PhotoShoots",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_PhotoShoots_CreatedByUserId",
                table: "PhotoShoots",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_IsDeleted",
                table: "Invoices",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_InvoiceId",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_PaymentDate",
                table: "Payments",
                column: "PaymentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Payment_TransactionId",
                table: "Payments",
                column: "TransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ProcessedByUserId",
                table: "Payments",
                column: "ProcessedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_PhotoShoots_AspNetUsers_CreatedByUserId",
                table: "PhotoShoots",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PhotoShoots_AspNetUsers_UpdatedByUserId",
                table: "PhotoShoots",
                column: "UpdatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PhotoShoots_AspNetUsers_CreatedByUserId",
                table: "PhotoShoots");

            migrationBuilder.DropForeignKey(
                name: "FK_PhotoShoots_AspNetUsers_UpdatedByUserId",
                table: "PhotoShoots");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_PhotoShoot_IsDeleted",
                table: "PhotoShoots");

            migrationBuilder.DropIndex(
                name: "IX_PhotoShoots_CreatedByUserId",
                table: "PhotoShoots");

            migrationBuilder.DropIndex(
                name: "IX_Invoice_IsDeleted",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "CreatedByUserId",
                table: "PhotoShoots");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "PhotoShoots");

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "PhotoShoots");

            migrationBuilder.DropColumn(
                name: "NextRecurrenceDate",
                table: "PhotoShoots");

            migrationBuilder.DropColumn(
                name: "RecurrencePattern",
                table: "PhotoShoots");

            migrationBuilder.DropColumn(
                name: "ShootType",
                table: "PhotoShoots");

            migrationBuilder.DropColumn(
                name: "Currency",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DepositAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DepositPaid",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "DepositPaidDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "IsRecurring",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "NextRecurrenceDate",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RecurrencePattern",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "ReminderSentDate",
                table: "Invoices");

            migrationBuilder.RenameColumn(
                name: "UpdatedByUserId",
                table: "PhotoShoots",
                newName: "PhotographerId");

            migrationBuilder.RenameIndex(
                name: "IX_PhotoShoots_UpdatedByUserId",
                table: "PhotoShoots",
                newName: "IX_PhotoShoots_PhotographerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PhotoShoots_AspNetUsers_PhotographerId",
                table: "PhotoShoots",
                column: "PhotographerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
