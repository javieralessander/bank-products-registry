using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BankProductsRegistry.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClientUserLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClientId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ClientId",
                table: "AspNetUsers",
                column: "ClientId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Clients_ClientId",
                table: "AspNetUsers",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Clients_ClientId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ClientId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "AspNetUsers");
        }
    }
}
