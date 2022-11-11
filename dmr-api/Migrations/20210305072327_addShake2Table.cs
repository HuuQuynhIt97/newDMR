using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class addShake2Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Shakes",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChemicalType = table.Column<string>(nullable: true),
                    StandardCycle = table.Column<double>(nullable: false),
                    ActualCycle = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: true),
                    EndTime = table.Column<DateTime>(nullable: true),
                    MixingInfoID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shakes", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Shakes_MixingInfos_MixingInfoID",
                        column: x => x.MixingInfoID,
                        principalTable: "MixingInfos",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Shakes_MixingInfoID",
                table: "Shakes",
                column: "MixingInfoID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Shakes");
        }
    }
}
