using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class modifyPeriodTbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LunchTimeID",
                table: "Period",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Period_LunchTimeID",
                table: "Period",
                column: "LunchTimeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Period_LunchTime_LunchTimeID",
                table: "Period",
                column: "LunchTimeID",
                principalTable: "LunchTime",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Period_LunchTime_LunchTimeID",
                table: "Period");

            migrationBuilder.DropIndex(
                name: "IX_Period_LunchTimeID",
                table: "Period");

            migrationBuilder.DropColumn(
                name: "LunchTimeID",
                table: "Period");
        }
    }
}
