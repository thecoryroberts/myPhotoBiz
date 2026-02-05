using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class AddContractVariables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "ContractVariables",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DefaultValue = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    IsSystemVariable = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractVariables", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractVariableValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContractId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContractVariableId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractVariableValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractVariableValues_ContractVariables_ContractVariableId",
                        column: x => x.ContractVariableId,
                        principalTable: "ContractVariables",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractVariableValues_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContractVariable_IsActive",
                table: "ContractVariables",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_ContractVariable_Name",
                table: "ContractVariables",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractVariableValue_Contract_Variable",
                table: "ContractVariableValues",
                columns: new[] { "ContractId", "ContractVariableId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractVariableValues_ContractVariableId",
                table: "ContractVariableValues",
                column: "ContractVariableId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContractVariableValues");

            migrationBuilder.DropTable(
                name: "ContractVariables");

            migrationBuilder.DropColumn(
                name: "GuardianName",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "PackageName",
                table: "Contracts");
        }
    }
}
