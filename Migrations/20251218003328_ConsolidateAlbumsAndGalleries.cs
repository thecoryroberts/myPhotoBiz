using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateAlbumsAndGalleries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Photos_Galleries_GalleryId",
                table: "Photos");

            migrationBuilder.DropIndex(
                name: "IX_Photo_GalleryId",
                table: "Photos");

            migrationBuilder.DropColumn(
                name: "GalleryId",
                table: "Photos");

            migrationBuilder.CreateTable(
                name: "GalleryAlbum",
                columns: table => new
                {
                    AlbumId = table.Column<int>(type: "INTEGER", nullable: false),
                    GalleryId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GalleryAlbum", x => new { x.AlbumId, x.GalleryId });
                    table.ForeignKey(
                        name: "FK_GalleryAlbum_Albums_AlbumId",
                        column: x => x.AlbumId,
                        principalTable: "Albums",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GalleryAlbum_Galleries_GalleryId",
                        column: x => x.GalleryId,
                        principalTable: "Galleries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GalleryAlbum_GalleryId",
                table: "GalleryAlbum",
                column: "GalleryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GalleryAlbum");

            migrationBuilder.AddColumn<int>(
                name: "GalleryId",
                table: "Photos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Photo_GalleryId",
                table: "Photos",
                column: "GalleryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Photos_Galleries_GalleryId",
                table: "Photos",
                column: "GalleryId",
                principalTable: "Galleries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
