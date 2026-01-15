using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class AddClientFolderLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Method",
                table: "Clients",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "FolderId",
                table: "ClientProfiles",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientProfiles_FolderId",
                table: "ClientProfiles",
                column: "FolderId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientProfiles_Files_FolderId",
                table: "ClientProfiles",
                column: "FolderId",
                principalTable: "Files",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientProfiles_Files_FolderId",
                table: "ClientProfiles");

            migrationBuilder.DropIndex(
                name: "IX_ClientProfiles_FolderId",
                table: "ClientProfiles");

            migrationBuilder.DropColumn(
                name: "Method",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "FolderId",
                table: "ClientProfiles");
        }
    }
}
