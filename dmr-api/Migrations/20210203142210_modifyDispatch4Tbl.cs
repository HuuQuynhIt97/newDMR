using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class modifyDispatch4Tbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dispatches_DispatchList_DispatchListID",
                table: "Dispatches");

            migrationBuilder.AlterColumn<int>(
                name: "DispatchListID",
                table: "Dispatches",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedFinishTime",
                table: "Dispatches",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedStartTime",
                table: "Dispatches",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "GlueNameID",
                table: "Dispatches",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Dispatches_DispatchList_DispatchListID",
                table: "Dispatches",
                column: "DispatchListID",
                principalTable: "DispatchList",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dispatches_DispatchList_DispatchListID",
                table: "Dispatches");

            migrationBuilder.DropColumn(
                name: "EstimatedFinishTime",
                table: "Dispatches");

            migrationBuilder.DropColumn(
                name: "EstimatedStartTime",
                table: "Dispatches");

            migrationBuilder.DropColumn(
                name: "GlueNameID",
                table: "Dispatches");

            migrationBuilder.AlterColumn<int>(
                name: "DispatchListID",
                table: "Dispatches",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Dispatches_DispatchList_DispatchListID",
                table: "Dispatches",
                column: "DispatchListID",
                principalTable: "DispatchList",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
