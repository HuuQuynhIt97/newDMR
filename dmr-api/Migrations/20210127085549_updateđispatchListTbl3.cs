using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace DMR_API.Migrations
{
    public partial class updateđispatchListTbl3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
              name: "StartTimeOfPeriod",
              table: "DispatchList",
              nullable: false);

            migrationBuilder.AddColumn<DateTime>(
              name: "FinishTimeOfPeriod",
              table: "DispatchList",
              nullable: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                           name: "StartTimeOfPeriod",
                           table: "DispatchList");
            migrationBuilder.DropColumn(
                          name: "FinishTimeOfPeriod",
                          table: "DispatchList");
        }
    }
}
