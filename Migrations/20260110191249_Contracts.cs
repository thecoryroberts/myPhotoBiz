using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class Contracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MinorModelReleases",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReleaseType = table.Column<int>(type: "INTEGER", nullable: false),
                    ClientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    MinorName = table.Column<string>(type: "TEXT", nullable: false),
                    GuardianName = table.Column<string>(type: "TEXT", nullable: false),
                    UsageRights = table.Column<int>(type: "INTEGER", nullable: false),
                    JurisdictionState = table.Column<string>(type: "TEXT", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AcceptanceMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    ContractVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    ClientProfileId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MinorModelReleases", x => x.id);
                    table.ForeignKey(
                        name: "FK_MinorModelReleases_ClientProfiles_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ModelReleases",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ReleaseType = table.Column<int>(type: "INTEGER", nullable: false),
                    ClientId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ModelName = table.Column<string>(type: "TEXT", nullable: false),
                    UsageRights = table.Column<int>(type: "INTEGER", nullable: false),
                    JurisdictionState = table.Column<string>(type: "TEXT", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    AcceptanceMethod = table.Column<int>(type: "INTEGER", nullable: false),
                    ContractVersion = table.Column<int>(type: "INTEGER", nullable: false),
                    IsRevoked = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsArchived = table.Column<bool>(type: "INTEGER", nullable: false),
                    ClientProfileId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModelReleases", x => x.id);
                    table.ForeignKey(
                        name: "FK_ModelReleases_ClientProfiles_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MinorModelReleases_ClientProfileId",
                table: "MinorModelReleases",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ModelReleases_ClientProfileId",
                table: "ModelReleases",
                column: "ClientProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MinorModelReleases");

            migrationBuilder.DropTable(
                name: "ModelReleases");
        }
    }
}
