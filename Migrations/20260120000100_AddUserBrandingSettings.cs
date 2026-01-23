using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBrandingSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BrandAccentColor",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrandCoverImageUrl",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrandLogoDarkUrl",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrandLogoLightUrl",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrandName",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrandPrimaryColor",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BrandTagline",
                table: "AspNetUsers",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BrandAccentColor",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BrandCoverImageUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BrandLogoDarkUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BrandLogoLightUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BrandName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BrandPrimaryColor",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "BrandTagline",
                table: "AspNetUsers");
        }
    }
}
