using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PLMWebApp.Migrations
{
    public partial class AddCODToReservationHeaderToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "COD",
                table: "ReservationHeaders",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "COD",
                table: "ReservationHeaders");
        }
    }
}
