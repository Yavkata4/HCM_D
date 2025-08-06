using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HCM_D.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAdminAndDepartmentGrowthBack : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add IsAdmin column to Employees table
            migrationBuilder.AddColumn<bool>(
                name: "IsAdmin",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Create DepartmentGrowths table
            migrationBuilder.CreateTable(
                name: "DepartmentGrowths",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DepartmentId = table.Column<int>(type: "int", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Revenue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Expenses = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DepartmentGoal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepartmentGrowths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DepartmentGrowths_Departments_DepartmentId",
                        column: x => x.DepartmentId,
                        principalTable: "Departments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create unique index for department and year combination
            migrationBuilder.CreateIndex(
                name: "IX_DepartmentGrowths_DepartmentId_Year",
                table: "DepartmentGrowths",
                columns: new[] { "DepartmentId", "Year" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepartmentGrowths_DepartmentId",
                table: "DepartmentGrowths",
                column: "DepartmentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop DepartmentGrowths table
            migrationBuilder.DropTable(
                name: "DepartmentGrowths");

            // Remove IsAdmin column from Employees table
            migrationBuilder.DropColumn(
                name: "IsAdmin",
                table: "Employees");
        }
    }
}