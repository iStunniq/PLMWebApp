using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PLMWebApp.Migrations
{
    public partial class AddCODToReservationHeaderToDbCanBeNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "COD",
                table: "ReservationHeaders",
                type: "bit",
                nullable: true,
                oldClrType: typeof(bool),
                oldType: "bit");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "COD",
                table: "ReservationHeaders",
                type: "bit",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldNullable: true);
        }
    }
}
