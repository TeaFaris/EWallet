using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EWallet.Data.Migrations
{
    /// <inheritdoc />
    public partial class walletidentified : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Identified",
                table: "Wallets",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Identified",
                table: "Wallets");
        }
    }
}
