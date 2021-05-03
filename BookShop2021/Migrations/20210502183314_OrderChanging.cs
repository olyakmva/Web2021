using Microsoft.EntityFrameworkCore.Migrations;

namespace BookShop2021.Migrations
{
    public partial class OrderChanging : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientId",
                table: "Orders",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TotalOrdersCost",
                table: "Clients",
                nullable: false,
                oldClrType: typeof(decimal));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Orders");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalOrdersCost",
                table: "Clients",
                nullable: false,
                oldClrType: typeof(int));
        }
    }
}
