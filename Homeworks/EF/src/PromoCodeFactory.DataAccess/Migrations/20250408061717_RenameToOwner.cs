using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PromoCodeFactory.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RenameToOwner : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromoCodes_Customers_CustomerInfoId",
                table: "PromoCodes");

            migrationBuilder.RenameColumn(
                name: "CustomerInfoId",
                table: "PromoCodes",
                newName: "OwnerId");

            migrationBuilder.RenameIndex(
                name: "IX_PromoCodes_CustomerInfoId",
                table: "PromoCodes",
                newName: "IX_PromoCodes_OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_PromoCodes_Customers_OwnerId",
                table: "PromoCodes",
                column: "OwnerId",
                principalTable: "Customers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PromoCodes_Customers_OwnerId",
                table: "PromoCodes");

            migrationBuilder.RenameColumn(
                name: "OwnerId",
                table: "PromoCodes",
                newName: "CustomerInfoId");

            migrationBuilder.RenameIndex(
                name: "IX_PromoCodes_OwnerId",
                table: "PromoCodes",
                newName: "IX_PromoCodes_CustomerInfoId");

            migrationBuilder.AddForeignKey(
                name: "FK_PromoCodes_Customers_CustomerInfoId",
                table: "PromoCodes",
                column: "CustomerInfoId",
                principalTable: "Customers",
                principalColumn: "Id");
        }
    }
}
