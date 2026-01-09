using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class newsuperadmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_Photos_ClientProfileId",
                table: "Photos",
                newName: "IX_Photo_ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_GalleryAccess_ExpiryDate",
                table: "GalleryAccesses",
                column: "ExpiryDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GalleryAccess_ExpiryDate",
                table: "GalleryAccesses");

            migrationBuilder.RenameIndex(
                name: "IX_Photo_ClientProfileId",
                table: "Photos",
                newName: "IX_Photos_ClientProfileId");
        }
    }
}
