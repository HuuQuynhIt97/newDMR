using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class RemoveBuildingIdStirTbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Stirs_Buildings_BuildingID",
                table: "Stirs");

            migrationBuilder.DropIndex(
                name: "IX_Stirs_BuildingID",
                table: "Stirs");

            migrationBuilder.DropColumn(
                name: "BuildingID",
                table: "Stirs");

            migrationBuilder.DropColumn(
                name: "MachineID",
                table: "Stirs");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BuildingID",
                table: "Stirs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MachineID",
                table: "Stirs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Stirs_BuildingID",
                table: "Stirs",
                column: "BuildingID");

            migrationBuilder.AddForeignKey(
                name: "FK_Stirs_Buildings_BuildingID",
                table: "Stirs",
                column: "BuildingID",
                principalTable: "Buildings",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
