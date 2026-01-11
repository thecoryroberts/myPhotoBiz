using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class AddContractTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContractTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ContentTemplate = table.Column<string>(type: "TEXT", nullable: true),
                    PdfFilePathTemplate = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AwardBadgeOnSign = table.Column<bool>(type: "INTEGER", nullable: false),
                    BadgeToAwardId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractTemplates_Badges_BadgeToAwardId",
                        column: x => x.BadgeToAwardId,
                        principalTable: "Badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractTemplate_Category",
                table: "ContractTemplates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTemplate_IsActive",
                table: "ContractTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTemplate_Name",
                table: "ContractTemplates",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTemplates_BadgeToAwardId",
                table: "ContractTemplates",
                column: "BadgeToAwardId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractTemplates");
        }
    }
}
