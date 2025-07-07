using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGB_Project.Migrations
{
    /// <inheritdoc />
    public partial class RenameDataPrevistaDevolucaoColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Rename the column from DataPrevistaDevolucao to DataDevolucao
            migrationBuilder.RenameColumn(
                name: "DataPrevistaDevolucao",
                table: "Emprestimos",
                newName: "DataDevolucao");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Rename it back if needed
            migrationBuilder.RenameColumn(
                name: "DataDevolucao",
                table: "Emprestimos",
                newName: "DataPrevistaDevolucao");
        }
    }
}
