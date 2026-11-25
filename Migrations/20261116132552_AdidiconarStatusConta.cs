using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lsfinan.Migrations
{
    /// <inheritdoc />
    public partial class AdidiconarStatusConta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Ativa",
                table: "Contas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativa",
                table: "Contas");
        }
    }
}
