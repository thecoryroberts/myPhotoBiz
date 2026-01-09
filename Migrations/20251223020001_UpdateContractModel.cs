using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class UpdateContractModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientName",
                table: "Contracts");

            migrationBuilder.AlterColumn<string>(
                name: "SignatureImagePath",
                table: "Contracts",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Contracts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PhotoShootId",
                table: "Contracts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SentDate",
                table: "Contracts",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ClientId",
                table: "Contracts",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_PhotoShootId",
                table: "Contracts",
                column: "PhotoShootId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_Clients_ClientId",
                table: "Contracts");

            migrationBuilder.DropForeignKey(
                name: "FK_Contracts_PhotoShoots_PhotoShootId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_ClientId",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_PhotoShootId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "PhotoShootId",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "SentDate",
                table: "Contracts");

            migrationBuilder.AlterColumn<string>(
                name: "SignatureImagePath",
                table: "Contracts",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientName",
                table: "Contracts",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
