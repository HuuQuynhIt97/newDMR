using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class UpdateRoleIDPermissionTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_Permisions",
                table: "Permisions");

            migrationBuilder.DropColumn(
                name: "UserID",
                table: "Permisions");

            migrationBuilder.AddColumn<int>(
                name: "RoleID",
                table: "Permisions",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Permisions",
                table: "Permisions",
                columns: new[] { "ActionID", "FunctionSystemID", "RoleID" });

            migrationBuilder.CreateIndex(
                name: "IX_Permisions_RoleID",
                table: "Permisions",
                column: "RoleID");

            migrationBuilder.AddForeignKey(
                name: "FK_Permisions_Roles_RoleID",
                table: "Permisions",
                column: "RoleID",
                principalTable: "Roles",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Permisions_Roles_RoleID",
                table: "Permisions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Permisions",
                table: "Permisions");

            migrationBuilder.DropIndex(
                name: "IX_Permisions_RoleID",
                table: "Permisions");

            migrationBuilder.DropColumn(
                name: "RoleID",
                table: "Permisions");

            migrationBuilder.AddColumn<int>(
                name: "UserID",
                table: "Permisions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Permisions",
                table: "Permisions",
                columns: new[] { "ActionID", "FunctionSystemID", "UserID" });
        }
    }
}
