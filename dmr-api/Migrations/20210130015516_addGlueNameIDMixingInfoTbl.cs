using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class addGlueNameIDMixingInfoTbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GlueNameID",
                table: "MixingInfos",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MixingInfos_GlueNameID",
                table: "MixingInfos",
                column: "GlueNameID");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchListDetail_DispatchListID",
                table: "DispatchListDetail",
                column: "DispatchListID");

            migrationBuilder.AddForeignKey(
                name: "FK_DispatchListDetail_DispatchList_DispatchListID",
                table: "DispatchListDetail",
                column: "DispatchListID",
                principalTable: "DispatchList",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MixingInfos_GlueName_GlueNameID",
                table: "MixingInfos",
                column: "GlueNameID",
                principalTable: "GlueName",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DispatchListDetail_DispatchList_DispatchListID",
                table: "DispatchListDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_MixingInfos_GlueName_GlueNameID",
                table: "MixingInfos");

            migrationBuilder.DropIndex(
                name: "IX_MixingInfos_GlueNameID",
                table: "MixingInfos");

            migrationBuilder.DropIndex(
                name: "IX_DispatchListDetail_DispatchListID",
                table: "DispatchListDetail");

            migrationBuilder.DropColumn(
                name: "GlueNameID",
                table: "MixingInfos");
        }
    }
}
