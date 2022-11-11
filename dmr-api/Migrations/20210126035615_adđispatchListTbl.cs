using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class adđispatchListTbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DispatchList",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlanID = table.Column<int>(nullable: false),
                    MixingInfoID = table.Column<int>(nullable: false),
                    GlueID = table.Column<int>(nullable: false),
                    GlueNameID = table.Column<int>(nullable: false),
                    BuildingID = table.Column<int>(nullable: false),
                    LineID = table.Column<int>(nullable: false),
                    BPFCID = table.Column<int>(nullable: false),
                    LineName = table.Column<string>(nullable: true),
                    GlueName = table.Column<string>(nullable: true),
                    Supplier = table.Column<string>(nullable: true),
                    Status = table.Column<bool>(nullable: false),
                    StartDispatchingTime = table.Column<DateTime>(nullable: true),
                    FinishDispatchingTime = table.Column<DateTime>(nullable: true),
                    PrintTime = table.Column<DateTime>(nullable: true),
                    StandardConsumption = table.Column<double>(nullable: false),
                    MixedConsumption = table.Column<double>(nullable: false),
                    DeliveredConsumption = table.Column<double>(nullable: false),
                    EstimatedStartTime = table.Column<DateTime>(nullable: false),
                    EstimatedFinishTime = table.Column<DateTime>(nullable: false),
                    IsDelete = table.Column<bool>(nullable: false),
                    DeleteTime = table.Column<DateTime>(nullable: false),
                    DeleteBy = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DispatchList", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DispatchList_GlueName_GlueNameID",
                        column: x => x.GlueNameID,
                        principalTable: "GlueName",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DispatchList_Plans_PlanID",
                        column: x => x.PlanID,
                        principalTable: "Plans",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DispatchList_GlueNameID",
                table: "DispatchList",
                column: "GlueNameID");

            migrationBuilder.CreateIndex(
                name: "IX_DispatchList_PlanID",
                table: "DispatchList",
                column: "PlanID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DispatchList");
        }
    }
}
