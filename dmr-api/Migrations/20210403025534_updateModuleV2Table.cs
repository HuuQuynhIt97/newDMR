using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class updateModuleV2Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FunctionSystem_Module_ModuleID",
                table: "FunctionSystem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Module",
                table: "Module");

            migrationBuilder.RenameTable(
                name: "Module",
                newName: "Modules");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Modules",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Modules",
                table: "Modules",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_FunctionSystem_Modules_ModuleID",
                table: "FunctionSystem",
                column: "ModuleID",
                principalTable: "Modules",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FunctionSystem_Modules_ModuleID",
                table: "FunctionSystem");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Modules",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Modules");

            migrationBuilder.RenameTable(
                name: "Modules",
                newName: "Module");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Module",
                table: "Module",
                column: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_FunctionSystem_Module_ModuleID",
                table: "FunctionSystem",
                column: "ModuleID",
                principalTable: "Module",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
