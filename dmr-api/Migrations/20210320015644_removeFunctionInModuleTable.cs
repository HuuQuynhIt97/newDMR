using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class removeFunctionInModuleTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FunctionInModule");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Permisions",
                table: "Permisions");

            migrationBuilder.AddColumn<int>(
                name: "ModuleID",
                table: "Permisions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Permisions",
                table: "Permisions",
                columns: new[] { "ModuleID", "ActionID", "FunctionSystemID", "UserID" });

            migrationBuilder.CreateIndex(
                name: "IX_Permisions_ActionID",
                table: "Permisions",
                column: "ActionID");

            migrationBuilder.AddForeignKey(
                name: "FK_Permisions_Module_ModuleID",
                table: "Permisions",
                column: "ModuleID",
                principalTable: "Module",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permisions_Module_ModuleID",
                table: "Permisions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Permisions",
                table: "Permisions");

            migrationBuilder.DropIndex(
                name: "IX_Permisions_ActionID",
                table: "Permisions");

            migrationBuilder.DropColumn(
                name: "ModuleID",
                table: "Permisions");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Permisions",
                table: "Permisions",
                columns: new[] { "ActionID", "FunctionSystemID", "UserID" });

            migrationBuilder.CreateTable(
                name: "FunctionInModule",
                columns: table => new
                {
                    FunctionID = table.Column<int>(type: "int", nullable: false),
                    ModuleID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionInModule", x => new { x.FunctionID, x.ModuleID });
                    table.ForeignKey(
                        name: "FK_FunctionInModule_FunctionSystem_FunctionID",
                        column: x => x.FunctionID,
                        principalTable: "FunctionSystem",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FunctionInModule_Module_ModuleID",
                        column: x => x.ModuleID,
                        principalTable: "Module",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FunctionInModule_ModuleID",
                table: "FunctionInModule",
                column: "ModuleID");
        }
    }
}
