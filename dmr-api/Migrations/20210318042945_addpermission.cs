using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class addpermission : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Actions",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actions", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FunctionSystem",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    Level = table.Column<int>(nullable: false),
                    Url = table.Column<string>(nullable: true),
                    Sequence = table.Column<int>(nullable: false),
                    ParentID = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FunctionSystem", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ActionInFunctionSystem",
                columns: table => new
                {
                    FunctionSystemID = table.Column<int>(nullable: false),
                    ActionID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActionInFunctionSystem", x => new { x.ActionID, x.FunctionSystemID });
                    table.ForeignKey(
                        name: "FK_ActionInFunctionSystem_Actions_ActionID",
                        column: x => x.ActionID,
                        principalTable: "Actions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ActionInFunctionSystem_FunctionSystem_FunctionSystemID",
                        column: x => x.FunctionSystemID,
                        principalTable: "FunctionSystem",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Permisions",
                columns: table => new
                {
                    UserID = table.Column<int>(nullable: false),
                    ActionID = table.Column<int>(nullable: false),
                    FunctionSystemID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permisions", x => new { x.ActionID, x.FunctionSystemID, x.UserID });
                    table.ForeignKey(
                        name: "FK_Permisions_Actions_ActionID",
                        column: x => x.ActionID,
                        principalTable: "Actions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Permisions_FunctionSystem_FunctionSystemID",
                        column: x => x.FunctionSystemID,
                        principalTable: "FunctionSystem",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActionInFunctionSystem_FunctionSystemID",
                table: "ActionInFunctionSystem",
                column: "FunctionSystemID");

            migrationBuilder.CreateIndex(
                name: "IX_Permisions_FunctionSystemID",
                table: "Permisions",
                column: "FunctionSystemID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActionInFunctionSystem");

            migrationBuilder.DropTable(
                name: "Permisions");

            migrationBuilder.DropTable(
                name: "Actions");

            migrationBuilder.DropTable(
                name: "FunctionSystem");
        }
    }
}
