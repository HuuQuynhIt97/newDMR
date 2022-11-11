using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class addDeliveredAmountđispatchListTbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveredConsumption",
                table: "DispatchList");

            migrationBuilder.DropColumn(
                name: "MixedConsumption",
                table: "DispatchList");

            migrationBuilder.DropColumn(
                name: "StandardConsumption",
                table: "DispatchList");

            migrationBuilder.AddColumn<double>(
                name: "DeliveredAmount",
                table: "DispatchList",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveredAmount",
                table: "DispatchList");

            migrationBuilder.AddColumn<double>(
                name: "DeliveredConsumption",
                table: "DispatchList",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "MixedConsumption",
                table: "DispatchList",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StandardConsumption",
                table: "DispatchList",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
