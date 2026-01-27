using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace myPhotoBiz.Migrations
{
    /// <inheritdoc />
    public partial class AddQuestionnaires : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionnaireTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    QuestionText = table.Column<string>(type: "TEXT", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuestionnaireAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    QuestionnaireTemplateId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedToUserId = table.Column<string>(type: "TEXT", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "TEXT", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Status = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionnaireAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionnaireAssignments_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuestionnaireAssignments_AspNetUsers_AssignedToUserId",
                        column: x => x.AssignedToUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_QuestionnaireAssignments_QuestionnaireTemplates_QuestionnaireTemplateId",
                        column: x => x.QuestionnaireTemplateId,
                        principalTable: "QuestionnaireTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireAssignment_AssignedToUserId",
                table: "QuestionnaireAssignments",
                column: "AssignedToUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireAssignment_Status",
                table: "QuestionnaireAssignments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireAssignment_TemplateId",
                table: "QuestionnaireAssignments",
                column: "QuestionnaireTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireAssignments_AssignedByUserId",
                table: "QuestionnaireAssignments",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplate_Category",
                table: "QuestionnaireTemplates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplate_IsActive",
                table: "QuestionnaireTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_QuestionnaireTemplate_Name",
                table: "QuestionnaireTemplates",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionnaireAssignments");

            migrationBuilder.DropTable(
                name: "QuestionnaireTemplates");
        }
    }
}
