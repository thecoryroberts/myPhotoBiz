using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class Photo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Photos",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Photos_ClientId",
                table: "Photos",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Clients_ClientId",
                table: "Photos",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Clients_ClientId",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Photos_ClientId",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Photos");
        }
    }
}
