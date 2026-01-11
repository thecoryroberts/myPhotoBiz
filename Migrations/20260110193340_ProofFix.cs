using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class ProofFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "Proofs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProofId",
                table: "PrintOrders",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Tags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tags", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FileItemTags",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FileItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    TagId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileItemTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FileItemTags_Files_FileItemId",
                        column: x => x.FileItemId,
                        principalTable: "Files",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FileItemTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Proofs_ClientId",
                table: "Proofs",
                column: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_PrintOrders_ProofId",
                table: "PrintOrders",
                column: "ProofId");

            migrationBuilder.CreateIndex(
                name: "IX_FileItemTags_FileItemId",
                table: "FileItemTags",
                column: "FileItemId");

            migrationBuilder.CreateIndex(
                name: "IX_FileItemTags_TagId",
                table: "FileItemTags",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_PrintOrders_Proofs_ProofId",
                table: "PrintOrders",
                column: "ProofId",
                principalTable: "Proofs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Proofs_Clients_ClientId",
                table: "Proofs",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PrintOrders_Proofs_ProofId",
                table: "PrintOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_Proofs_Clients_ClientId",
                table: "Proofs");

            migrationBuilder.DropTable(
                name: "FileItemTags");

            migrationBuilder.DropTable(
                name: "Tags");

            migrationBuilder.DropIndex(
                name: "IX_Proofs_ClientId",
                table: "Proofs");

            migrationBuilder.DropIndex(
                name: "IX_PrintOrders_ProofId",
                table: "PrintOrders");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Proofs");

            migrationBuilder.DropColumn(
                name: "ProofId",
                table: "PrintOrders");
        }
    }
}
