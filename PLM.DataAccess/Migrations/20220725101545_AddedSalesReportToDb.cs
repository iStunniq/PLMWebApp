using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PLMWebApp.Migrations
{
    public partial class AddedSalesReportToDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SalesReports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MinDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MaxDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReservationAmount = table.Column<int>(type: "int", nullable: false),
                    Overhead = table.Column<double>(type: "float", nullable: false),
                    BaseCosts = table.Column<double>(type: "float", nullable: false),
                    GrossIncome = table.Column<double>(type: "float", nullable: false),
                    NetIncome = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SalesReports", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SalesReports");
        }
    }
}
