using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class updatePlanTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOffline",
                table: "Plans",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedBy",
                table: "Plans",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedNoOvertime",
                table: "Plans",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedNoOvertimeBy",
                table: "Plans",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOffline",
                table: "Plans",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedOfflineBy",
                table: "Plans",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOnline",
                table: "Plans",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedOnlineBy",
                table: "Plans",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedOvertime",
                table: "Plans",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UpdatedOvertimeBy",
                table: "Plans",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOffline",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "UpdatedBy",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "UpdatedNoOvertime",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "UpdatedNoOvertimeBy",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "UpdatedOffline",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "UpdatedOfflineBy",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "UpdatedOnline",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "UpdatedOnlineBy",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "UpdatedOvertime",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "UpdatedOvertimeBy",
                table: "Plans");
        }
    }
}
