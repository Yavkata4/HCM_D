using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HCM_D.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeNumberAndHireDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ChangedByEmail",
                table: "SalaryHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChangedByEmployeeId",
                table: "SalaryHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ChangedByFullName",
                table: "SalaryHistories",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "EmployeeNumber",
                table: "Employees",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "HireDate",
                table: "Employees",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            // Update existing employees with unique employee numbers and default hire dates
            migrationBuilder.Sql(@"
                UPDATE Employees 
                SET EmployeeNumber = 'EMP' + FORMAT(Id, '000'),
                    HireDate = COALESCE(HireDate, GETUTCDATE())
                WHERE EmployeeNumber = '' OR EmployeeNumber IS NULL;
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeNumber",
                table: "Employees",
                column: "EmployeeNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_EmployeeNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ChangedByEmail",
                table: "SalaryHistories");

            migrationBuilder.DropColumn(
                name: "ChangedByEmployeeId",
                table: "SalaryHistories");

            migrationBuilder.DropColumn(
                name: "ChangedByFullName",
                table: "SalaryHistories");

            migrationBuilder.DropColumn(
                name: "EmployeeNumber",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "HireDate",
                table: "Employees");
        }
    }
}
