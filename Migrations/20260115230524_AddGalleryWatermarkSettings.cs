using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class AddGalleryWatermarkSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WatermarkEnabled",
                table: "Galleries",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "WatermarkImagePath",
                table: "Galleries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "WatermarkOpacity",
                table: "Galleries",
                type: "REAL",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.AddColumn<int>(
                name: "WatermarkPosition",
                table: "Galleries",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "WatermarkText",
                table: "Galleries",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "WatermarkTiled",
                table: "Galleries",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WatermarkEnabled",
                table: "Galleries");

            migrationBuilder.DropColumn(
                name: "WatermarkImagePath",
                table: "Galleries");

            migrationBuilder.DropColumn(
                name: "WatermarkOpacity",
                table: "Galleries");

            migrationBuilder.DropColumn(
                name: "WatermarkPosition",
                table: "Galleries");

            migrationBuilder.DropColumn(
                name: "WatermarkText",
                table: "Galleries");

            migrationBuilder.DropColumn(
                name: "WatermarkTiled",
                table: "Galleries");
        }
    }
}
