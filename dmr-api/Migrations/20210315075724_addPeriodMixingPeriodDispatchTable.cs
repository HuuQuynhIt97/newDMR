using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class addPeriodMixingPeriodDispatchTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_LunchTime_Buildings_BuildingID",
            //    table: "LunchTime");

            migrationBuilder.DropTable(
                name: "Period");

            //migrationBuilder.DropIndex(
            //    name: "IX_LunchTime_BuildingID",
            //    table: "LunchTime");

            migrationBuilder.DropColumn(
                name: "BuildingID",
                table: "LunchTime");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedTime",
                table: "LunchTime",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "DeletedBy",
                table: "LunchTime",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedTime",
                table: "LunchTime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IsDelete",
                table: "LunchTime",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LunchTimeID",
                table: "Buildings",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PeriodMixing",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingID = table.Column<int>(nullable: false),
                    IsOvertime = table.Column<bool>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    UpdatedTime = table.Column<DateTime>(nullable: true),
                    DeletedTime = table.Column<DateTime>(nullable: true),
                    IsDelete = table.Column<int>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    DeletedBy = table.Column<int>(nullable: false),
                    UpdatedBy = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodMixing", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PeriodMixing_Buildings_BuildingID",
                        column: x => x.BuildingID,
                        principalTable: "Buildings",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PeriodDispatch",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: false),
                    IsDelete = table.Column<bool>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false),
                    DeletedBy = table.Column<int>(nullable: false),
                    UpdatedBy = table.Column<int>(nullable: false),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    DeletedTime = table.Column<DateTime>(nullable: true),
                    UpdatedTime = table.Column<DateTime>(nullable: true),
                    PeriodMixingID = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodDispatch", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PeriodDispatch_PeriodMixing_PeriodMixingID",
                        column: x => x.PeriodMixingID,
                        principalTable: "PeriodMixing",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_LunchTimeID",
                table: "Buildings",
                column: "LunchTimeID");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodDispatch_PeriodMixingID",
                table: "PeriodDispatch",
                column: "PeriodMixingID");

            migrationBuilder.CreateIndex(
                name: "IX_PeriodMixing_BuildingID",
                table: "PeriodMixing",
                column: "BuildingID");

            migrationBuilder.AddForeignKey(
                name: "FK_Buildings_LunchTime_LunchTimeID",
                table: "Buildings",
                column: "LunchTimeID",
                principalTable: "LunchTime",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Buildings_LunchTime_LunchTimeID",
                table: "Buildings");

            migrationBuilder.DropTable(
                name: "PeriodDispatch");

            migrationBuilder.DropTable(
                name: "PeriodMixing");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_LunchTimeID",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "DeletedBy",
                table: "LunchTime");

            migrationBuilder.DropColumn(
                name: "DeletedTime",
                table: "LunchTime");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                table: "LunchTime");

            migrationBuilder.DropColumn(
                name: "LunchTimeID",
                table: "Buildings");

            migrationBuilder.AlterColumn<DateTime>(
                name: "UpdatedTime",
                table: "LunchTime",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BuildingID",
                table: "LunchTime",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Period",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedBy = table.Column<int>(type: "int", nullable: false),
                    CreatedTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsOvertime = table.Column<bool>(type: "bit", nullable: false),
                    LunchTimeID = table.Column<int>(type: "int", nullable: false),
                    Sequence = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<int>(type: "int", nullable: false),
                    UpdatedTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Period", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Period_LunchTime_LunchTimeID",
                        column: x => x.LunchTimeID,
                        principalTable: "LunchTime",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            //migrationBuilder.CreateIndex(
            //    name: "IX_LunchTime_BuildingID",
            //    table: "LunchTime",
            //    column: "BuildingID",
            //    unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Period_LunchTimeID",
                table: "Period",
                column: "LunchTimeID");

            //migrationBuilder.AddForeignKey(
            //    name: "FK_LunchTime_Buildings_BuildingID",
            //    table: "LunchTime",
            //    column: "BuildingID",
            //    principalTable: "Buildings",
            //    principalColumn: "ID",
            //    onDelete: ReferentialAction.Cascade);
        }
    }
}
