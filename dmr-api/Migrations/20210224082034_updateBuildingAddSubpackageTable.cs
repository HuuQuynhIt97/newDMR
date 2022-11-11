using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class updateBuildingAddSubpackageTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEVA_UV",
                table: "ToDoList",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Period",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedTime",
                table: "Period",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Period",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedTime",
                table: "Period",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "LunchTime",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedTime",
                table: "LunchTime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "LunchTime",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedTime",
                table: "LunchTime",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Dispatches",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "RemainingAmount",
                table: "Dispatches",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "KindID",
                table: "Buildings",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Subpackages",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<int>(nullable: false),
                    GlueNameID = table.Column<int>(nullable: false),
                    MixingInfoID = table.Column<int>(nullable: false),
                    GlueName = table.Column<string>(nullable: true),
                    Amount = table.Column<double>(nullable: false),
                    CreatedTime = table.Column<DateTime>(nullable: false),
                    CreatedBy = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subpackages", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_KindID",
                table: "Buildings",
                column: "KindID");

            migrationBuilder.AddForeignKey(
                name: "FK_Buildings_Kinds_KindID",
                table: "Buildings",
                column: "KindID",
                principalTable: "Kinds",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Buildings_Kinds_KindID",
                table: "Buildings");

            migrationBuilder.DropTable(
                name: "Subpackages");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_KindID",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "IsEVA_UV",
                table: "ToDoList");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Period");

            migrationBuilder.DropColumn(
                name: "CreatedTime",
                table: "Period");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Period");

            migrationBuilder.DropColumn(
                name: "UpdatedTime",
                table: "Period");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "LunchTime");

            migrationBuilder.DropColumn(
                name: "CreatedTime",
                table: "LunchTime");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "LunchTime");

            migrationBuilder.DropColumn(
                name: "UpdatedTime",
                table: "LunchTime");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Dispatches");

            migrationBuilder.DropColumn(
                name: "RemainingAmount",
                table: "Dispatches");

            migrationBuilder.DropColumn(
                name: "KindID",
                table: "Buildings");
        }
    }
}
