using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class addExpriedTimeSubpackageTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Subpackages_MixingInfoID",
                table: "Subpackages",
                column: "MixingInfoID");

            migrationBuilder.AddForeignKey(
                name: "FK_Subpackages_MixingInfos_MixingInfoID",
                table: "Subpackages",
                column: "MixingInfoID",
                principalTable: "MixingInfos",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Subpackages_MixingInfos_MixingInfoID",
                table: "Subpackages");

            migrationBuilder.DropIndex(
                name: "IX_Subpackages_MixingInfoID",
                table: "Subpackages");
        }
    }
}
