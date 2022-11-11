using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class modifyDispatchTbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteBy",
                table: "Dispatches");

            migrationBuilder.DropColumn(
                name: "DeleteTime",
                table: "Dispatches");

            migrationBuilder.DropColumn(
                name: "FinishDispatchingTime",
                table: "Dispatches");

            migrationBuilder.DropColumn(
                name: "IsDelete",
                table: "Dispatches");

            migrationBuilder.DropColumn(
                name: "StandardAmount",
                table: "Dispatches");

            migrationBuilder.DropColumn(
                name: "StartDispatchingTime",
                table: "Dispatches");

            migrationBuilder.DropColumn(
                name: "StationID",
                table: "Dispatches");

            migrationBuilder.AddColumn<DateTime>(
                name: "Time_Start",
                table: "MixingInfoDetails",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DispatchListID",
                table: "Dispatches",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Dispatches_DispatchListID",
                table: "Dispatches",
                column: "DispatchListID");

            migrationBuilder.AddForeignKey(
                name: "FK_Dispatches_DispatchList_DispatchListID",
                table: "Dispatches",
                column: "DispatchListID",
                principalTable: "DispatchList",
                principalColumn: "ID",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dispatches_DispatchList_DispatchListID",
                table: "Dispatches");

            migrationBuilder.DropIndex(
                name: "IX_Dispatches_DispatchListID",
                table: "Dispatches");

            migrationBuilder.DropColumn(
                name: "Time_Start",
                table: "MixingInfoDetails");

            migrationBuilder.DropColumn(
                name: "DispatchListID",
                table: "Dispatches");

            migrationBuilder.AddColumn<int>(
                name: "DeleteBy",
                table: "Dispatches",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeleteTime",
                table: "Dispatches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishDispatchingTime",
                table: "Dispatches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDelete",
                table: "Dispatches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "StandardAmount",
                table: "Dispatches",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartDispatchingTime",
                table: "Dispatches",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StationID",
                table: "Dispatches",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
