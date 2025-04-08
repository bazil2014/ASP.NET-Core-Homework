using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromoCodeFactory.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CascadeDeleteForCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromoCodes_Customers_OwnerId",
                table: "PromoCodes");

            migrationBuilder.AddForeignKey(
                name: "FK_PromoCodes_Customers_OwnerId",
                table: "PromoCodes",
                column: "OwnerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromoCodes_Customers_OwnerId",
                table: "PromoCodes");

            migrationBuilder.AddForeignKey(
                name: "FK_PromoCodes_Customers_OwnerId",
                table: "PromoCodes",
                column: "OwnerId",
                principalTable: "Customers",
                principalColumn: "Id");
        }
    }
}
