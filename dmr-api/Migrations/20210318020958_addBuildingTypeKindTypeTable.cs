using Microsoft.EntityFrameworkCore.Migrations;

namespace DMR_API.Migrations
{
    public partial class addBuildingTypeKindTypeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "KindTypeID",
                table: "Kinds",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BuildingTypeID",
                table: "Buildings",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BuildingType",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuildingType", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "KindType",
                columns: table => new
                {
                    ID = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KindType", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Kinds_KindTypeID",
                table: "Kinds",
                column: "KindTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_Buildings_BuildingTypeID",
                table: "Buildings",
                column: "BuildingTypeID");

            migrationBuilder.AddForeignKey(
                name: "FK_Buildings_BuildingType_BuildingTypeID",
                table: "Buildings",
                column: "BuildingTypeID",
                principalTable: "BuildingType",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Kinds_KindType_KindTypeID",
                table: "Kinds",
                column: "KindTypeID",
                principalTable: "KindType",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Buildings_BuildingType_BuildingTypeID",
                table: "Buildings");

            migrationBuilder.DropForeignKey(
                name: "FK_Kinds_KindType_KindTypeID",
                table: "Kinds");

            migrationBuilder.DropTable(
                name: "BuildingType");

            migrationBuilder.DropTable(
                name: "KindType");

            migrationBuilder.DropIndex(
                name: "IX_Kinds_KindTypeID",
                table: "Kinds");

            migrationBuilder.DropIndex(
                name: "IX_Buildings_BuildingTypeID",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "KindTypeID",
                table: "Kinds");

            migrationBuilder.DropColumn(
                name: "BuildingTypeID",
                table: "Buildings");
        }
    }
}
