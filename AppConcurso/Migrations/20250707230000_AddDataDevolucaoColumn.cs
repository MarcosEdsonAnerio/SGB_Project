using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGB_Project.Migrations
{
    /// <inheritdoc />
    public partial class AddDataDevolucaoColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // First check if the column exists to avoid errors
            migrationBuilder.Sql(
                @"SET @columnExists = 0;
                  SELECT COUNT(*) INTO @columnExists 
                  FROM INFORMATION_SCHEMA.COLUMNS 
                  WHERE TABLE_NAME = 'Emprestimos' AND COLUMN_NAME = 'DataDevolucao';
                  
                  SET @sql = IF(@columnExists = 0, 
                      'ALTER TABLE Emprestimos ADD COLUMN DataDevolucao datetime(6) NULL', 
                      'SELECT 1');
                  
                  PREPARE stmt FROM @sql;
                  EXECUTE stmt;
                  DEALLOCATE PREPARE stmt;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Don't drop the column in down migration to prevent data loss
        }
    }
}
