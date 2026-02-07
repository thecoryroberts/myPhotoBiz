using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionnaireDocumentSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "QuestionText",
                table: "QuestionnaireTemplates",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "DocumentPath",
                table: "QuestionnaireTemplates",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentType",
                table: "QuestionnaireTemplates",
                type: "TEXT",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "QuestionnaireTemplates",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "QuestionnaireTemplates",
                type: "TEXT",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentPath",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropColumn(
                name: "DocumentType",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "QuestionnaireTemplates");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "QuestionnaireTemplates");

            migrationBuilder.AlterColumn<string>(
                name: "QuestionText",
                table: "QuestionnaireTemplates",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
