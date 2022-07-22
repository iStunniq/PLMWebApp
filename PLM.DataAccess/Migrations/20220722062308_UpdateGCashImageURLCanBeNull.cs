using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PLMWebApp.Migrations
{
    public partial class UpdateGCashImageURLCanBeNull : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GCashImageUrl",
                table: "ReservationHeaders",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "GCashImageUrl",
                table: "ReservationHeaders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }
    }
}
