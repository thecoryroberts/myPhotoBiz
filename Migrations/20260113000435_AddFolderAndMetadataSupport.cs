using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class AddFolderAndMetadataSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Proofs_Clients_ClientId",
                table: "Proofs");

            migrationBuilder.DropIndex(
                name: "IX_Proofs_ClientId",
                table: "Proofs");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Proofs");

            migrationBuilder.AlterColumn<string>(
                name: "ModelName",
                table: "ModelReleases",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "JurisdictionState",
                table: "ModelReleases",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<DateTime>(
                name: "Created",
                table: "Files",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Files",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DownloadCount",
                table: "Files",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsFavorite",
                table: "Files",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFolder",
                table: "Files",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAccessed",
                table: "Files",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "Files",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentFolderId",
                table: "Files",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Tags",
                table: "Files",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Files_ParentFolderId",
                table: "Files",
                column: "ParentFolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_Files_ParentFolderId",
                table: "Files",
                column: "ParentFolderId",
                principalTable: "Files",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_Files_ParentFolderId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_ParentFolderId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "DownloadCount",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "IsFavorite",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "IsFolder",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "LastAccessed",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "ParentFolderId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "Tags",
                table: "Files");

            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Proofs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ModelName",
                table: "ModelReleases",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "JurisdictionState",
                table: "ModelReleases",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Proofs_ClientId",
                table: "Proofs",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Proofs_Clients_ClientId",
                table: "Proofs",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");
        }
    }
}
