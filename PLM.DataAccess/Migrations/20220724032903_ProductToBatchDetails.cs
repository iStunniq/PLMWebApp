using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PLMWebApp.Migrations
{
    public partial class ProductToBatchDetails : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationDetails_Products_ProductId",
                table: "ReservationDetails");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "ReservationDetails",
                newName: "BatchId");

            migrationBuilder.RenameIndex(
                name: "IX_ReservationDetails_ProductId",
                table: "ReservationDetails",
                newName: "IX_ReservationDetails_BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationDetails_Batches_BatchId",
                table: "ReservationDetails",
                column: "BatchId",
                principalTable: "Batches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservationDetails_Batches_BatchId",
                table: "ReservationDetails");

            migrationBuilder.RenameColumn(
                name: "BatchId",
                table: "ReservationDetails",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ReservationDetails_BatchId",
                table: "ReservationDetails",
                newName: "IX_ReservationDetails_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservationDetails_Products_ProductId",
                table: "ReservationDetails",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
