using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class updatePermissionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Permisions",
                table: "Permisions");

            migrationBuilder.AddColumn<int>(
                name: "ModuleID",
                table: "Permisions",
                type: "int",
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
    }
}
