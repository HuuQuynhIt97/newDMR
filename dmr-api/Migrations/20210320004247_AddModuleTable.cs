using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class AddModuleTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FunctionInModule_FunctionSystem_FunctionSystemID",
                table: "FunctionInModule");

            migrationBuilder.DropForeignKey(
                name: "FK_FunctionInModule_FunctionSystem_ModuleID",
                table: "FunctionInModule");

            migrationBuilder.DropIndex(
                name: "IX_FunctionInModule_FunctionSystemID",
                table: "FunctionInModule");

            migrationBuilder.DropColumn(
                name: "FunctionSystemID",
                table: "FunctionInModule");

            migrationBuilder.AddColumn<int>(
                name: "ModuleID",
                table: "FunctionSystem",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Module",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    Sequence = table.Column<int>(nullable: false),
                    CreatedTime = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Module", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FunctionSystem_ModuleID",
                table: "FunctionSystem",
                column: "ModuleID");

            migrationBuilder.CreateIndex(
                name: "IX_FunctionSystem_ParentID",
                table: "FunctionSystem",
                column: "ParentID");

            migrationBuilder.AddForeignKey(
                name: "FK_FunctionInModule_FunctionSystem_FunctionID",
                table: "FunctionInModule",
                column: "FunctionID",
                principalTable: "FunctionSystem",
                principalColumn: "ID",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_FunctionInModule_Module_ModuleID",
                table: "FunctionInModule",
                column: "ModuleID",
                principalTable: "Module",
                principalColumn: "ID",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_FunctionSystem_Module_ModuleID",
                table: "FunctionSystem",
                column: "ModuleID",
                principalTable: "Module",
                principalColumn: "ID",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_FunctionSystem_FunctionSystem_ParentID",
                table: "FunctionSystem",
                column: "ParentID",
                principalTable: "FunctionSystem",
                principalColumn: "ID",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FunctionInModule_FunctionSystem_FunctionID",
                table: "FunctionInModule");

            migrationBuilder.DropForeignKey(
                name: "FK_FunctionInModule_Module_ModuleID",
                table: "FunctionInModule");

            migrationBuilder.DropForeignKey(
                name: "FK_FunctionSystem_Module_ModuleID",
                table: "FunctionSystem");

            migrationBuilder.DropForeignKey(
                name: "FK_FunctionSystem_FunctionSystem_ParentID",
                table: "FunctionSystem");

            migrationBuilder.DropTable(
                name: "Module");

            migrationBuilder.DropIndex(
                name: "IX_FunctionSystem_ModuleID",
                table: "FunctionSystem");

            migrationBuilder.DropIndex(
                name: "IX_FunctionSystem_ParentID",
                table: "FunctionSystem");

            migrationBuilder.DropColumn(
                name: "ModuleID",
                table: "FunctionSystem");

            migrationBuilder.AddColumn<int>(
                name: "FunctionSystemID",
                table: "FunctionInModule",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FunctionInModule_FunctionSystemID",
                table: "FunctionInModule",
                column: "FunctionSystemID");

            migrationBuilder.AddForeignKey(
                name: "FK_FunctionInModule_FunctionSystem_FunctionSystemID",
                table: "FunctionInModule",
                column: "FunctionSystemID",
                principalTable: "FunctionSystem",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FunctionInModule_FunctionSystem_ModuleID",
                table: "FunctionInModule",
                column: "ModuleID",
                principalTable: "FunctionSystem",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
