using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class AddAppSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContractVariableValues_ContractVariables_ContractVariableId",
                table: "ContractVariableValues");

            migrationBuilder.CreateTable(
                name: "AppSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    BusinessName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Tagline = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    BusinessDescription = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    BusinessEmail = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    BusinessPhone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    BusinessAddress = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    FacebookUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    InstagramUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    TwitterUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    LinkedInUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    PinterestUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    YouTubeUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    LogoPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    LogoDarkPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    FaviconPath = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    PrimaryColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    SecondaryColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    AccentColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    SuccessColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    WarningColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    DangerColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    InvoiceHeaderColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    InvoiceAccentColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    InvoiceTextColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    InvoiceNumberPrefix = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    DefaultPaymentTermsDays = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultTaxRate = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    CurrencySymbol = table.Column<string>(type: "TEXT", maxLength: 5, nullable: false),
                    InvoiceFooterText = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    InvoiceTermsText = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ShowLogoOnInvoice = table.Column<bool>(type: "INTEGER", nullable: false),
                    ContractHeaderColor = table.Column<string>(type: "TEXT", maxLength: 7, nullable: false),
                    ContractNumberPrefix = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ShowLogoOnContract = table.Column<bool>(type: "INTEGER", nullable: false),
                    BookingReferencePrefix = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    RequireBookingDeposit = table.Column<bool>(type: "INTEGER", nullable: false),
                    DefaultDepositPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    AllowSameDayBookings = table.Column<bool>(type: "INTEGER", nullable: false),
                    MinimumBookingNoticeHours = table.Column<int>(type: "INTEGER", nullable: false),
                    DefaultGalleryExpiryDays = table.Column<int>(type: "INTEGER", nullable: false),
                    AllowGalleryDownloads = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnableGalleryWatermarks = table.Column<bool>(type: "INTEGER", nullable: false),
                    DefaultPhotosPerPage = table.Column<int>(type: "INTEGER", nullable: false),
                    EmailSenderName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    EmailSignature = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    SendBookingConfirmations = table.Column<bool>(type: "INTEGER", nullable: false),
                    SendInvoiceReminders = table.Column<bool>(type: "INTEGER", nullable: false),
                    InvoiceReminderDays = table.Column<int>(type: "INTEGER", nullable: false),
                    Timezone = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DateFormat = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    TimeFormat = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    EnableTwoFactorAuth = table.Column<bool>(type: "INTEGER", nullable: false),
                    SessionTimeoutMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppSettings_AspNetUsers_UpdatedByUserId",
                        column: x => x.UpdatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppSettings_UpdatedByUserId",
                table: "AppSettings",
                column: "UpdatedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContractVariableValues_ContractVariables_ContractVariableId",
                table: "ContractVariableValues",
                column: "ContractVariableId",
                principalTable: "ContractVariables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContractVariableValues_ContractVariables_ContractVariableId",
                table: "ContractVariableValues");

            migrationBuilder.DropTable(
                name: "AppSettings");

            migrationBuilder.AddForeignKey(
                name: "FK_ContractVariableValues_ContractVariables_ContractVariableId",
                table: "ContractVariableValues",
                column: "ContractVariableId",
                principalTable: "ContractVariables",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
