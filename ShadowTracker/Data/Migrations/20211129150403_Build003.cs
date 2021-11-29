using Microsoft.EntityFrameworkCore.Migrations;

namespace ShadowTracker.Data.Migrations
{
    public partial class Build003 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_AspNetUsers_BTUserId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_BTUserId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "BTUserId",
                table: "Projects");

            migrationBuilder.CreateTable(
                name: "BTUserProject",
                columns: table => new
                {
                    MembersId = table.Column<string>(type: "text", nullable: false),
                    ProjectsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BTUserProject", x => new { x.MembersId, x.ProjectsId });
                    table.ForeignKey(
                        name: "FK_BTUserProject_AspNetUsers_MembersId",
                        column: x => x.MembersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BTUserProject_Projects_ProjectsId",
                        column: x => x.ProjectsId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BTUserProject_ProjectsId",
                table: "BTUserProject",
                column: "ProjectsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BTUserProject");

            migrationBuilder.AddColumn<string>(
                name: "BTUserId",
                table: "Projects",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_BTUserId",
                table: "Projects",
                column: "BTUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_AspNetUsers_BTUserId",
                table: "Projects",
                column: "BTUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
