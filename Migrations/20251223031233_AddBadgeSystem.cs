using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class AddBadgeSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Clients_ClientId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_PhotoShoots_PhotoShootId",
                table: "Contracts");

            migrationBuilder.AddColumn<bool>(
                name: "AwardBadgeOnSign",
                table: "Contracts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "BadgeToAwardId",
                table: "Contracts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PdfFilePath",
                table: "Contracts",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Icon = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Color = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoAward = table.Column<bool>(type: "INTEGER", nullable: false),
                    RequiredContractId = table.Column<int>(type: "INTEGER", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ClientBadges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClientId = table.Column<int>(type: "INTEGER", nullable: false),
                    BadgeId = table.Column<int>(type: "INTEGER", nullable: false),
                    EarnedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ContractId = table.Column<int>(type: "INTEGER", nullable: true),
                    Notes = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientBadges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientBadges_Badges_BadgeId",
                        column: x => x.BadgeId,
                        principalTable: "Badges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientBadges_Clients_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Clients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClientBadges_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_BadgeToAwardId",
                table: "Contracts",
                column: "BadgeToAwardId");

            migrationBuilder.CreateIndex(
                name: "IX_Badges_RequiredContractId",
                table: "Badges",
                column: "RequiredContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientBadges_BadgeId",
                table: "ClientBadges",
                column: "BadgeId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientBadges_ClientId",
                table: "ClientBadges",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientBadges_ContractId",
                table: "ClientBadges",
                column: "ContractId");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Badges_BadgeToAwardId",
                table: "Contracts",
                column: "BadgeToAwardId",
                principalTable: "Badges",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Clients_ClientId",
                table: "Contracts",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_PhotoShoots_PhotoShootId",
                table: "Contracts",
                column: "PhotoShootId",
                principalTable: "PhotoShoots",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Badges_BadgeToAwardId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Clients_ClientId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_PhotoShoots_PhotoShootId",
                table: "Contracts");

            migrationBuilder.DropTable(
                name: "ClientBadges");

            migrationBuilder.DropTable(
                name: "Badges");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_BadgeToAwardId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "AwardBadgeOnSign",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "BadgeToAwardId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "PdfFilePath",
                table: "Contracts");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_Clients_ClientId",
                table: "Contracts",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Contracts_PhotoShoots_PhotoShootId",
                table: "Contracts",
                column: "PhotoShootId",
                principalTable: "PhotoShoots",
                principalColumn: "Id");
        }
    }
}
