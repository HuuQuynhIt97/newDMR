using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class AddFunctionInModuleTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FunctionInModule",
                columns: table => new
                {
                    FunctionID = table.Column<int>(nullable: false),
                    ModuleID = table.Column<int>(nullable: false),
                    FunctionSystemID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionInModule", x => new { x.FunctionID, x.ModuleID });
                    table.ForeignKey(
                        name: "FK_FunctionInModule_FunctionSystem_FunctionSystemID",
                        column: x => x.FunctionSystemID,
                        principalTable: "FunctionSystem",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FunctionInModule_FunctionSystem_ModuleID",
                        column: x => x.ModuleID,
                        principalTable: "FunctionSystem",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FunctionInModule_FunctionSystemID",
                table: "FunctionInModule",
                column: "FunctionSystemID");

            migrationBuilder.CreateIndex(
                name: "IX_FunctionInModule_ModuleID",
                table: "FunctionInModule",
                column: "ModuleID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FunctionInModule");
        }
    }
}
