using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class UpdateStirTbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BuildingID",
                table: "Stirs",
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

        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
