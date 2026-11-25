using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lsfinan.Migrations
{
    /// <inheritdoc />
    public partial class RemoverRecebimentoEPagamentoFuturo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PagarDepois",
                table: "Saidas");

            migrationBuilder.DropColumn(
                name: "DataRecebimento",
                table: "Entradas");

            migrationBuilder.DropColumn(
                name: "ReceberDepois",
                table: "Entradas");

            migrationBuilder.DropColumn(
                name: "Recebido",
                table: "Entradas");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PagarDepois",
                table: "Saidas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataRecebimento",
                table: "Entradas",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ReceberDepois",
                table: "Entradas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Recebido",
                table: "Entradas",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }
    }
}
