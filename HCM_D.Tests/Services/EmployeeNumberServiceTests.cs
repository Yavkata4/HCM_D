using HCM_D.Data;
using HCM_D.Models;
using HCM_D.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace HCM_D.Tests.Services
{
    public class EmployeeNumberServiceTests : IDisposable
    {
        private readonly ApplicationDbContext _context;
        private readonly EmployeeNumberService _service;

        public EmployeeNumberServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _service = new EmployeeNumberService(_context);
        }

        [Fact]
        public async Task GenerateEmployeeNumberAsync_WithNoExistingEmployees_ReturnsEMP1001()
        {
            // Act
            var result = await _service.GenerateEmployeeNumberAsync();

            // Assert
            Assert.Equal("EMP-1001", result);
        }

        [Fact]
        public async Task GenerateEmployeeNumberAsync_WithExistingEmployees_ReturnsNextNumber()
        {
            // Arrange
            _context.Employees.Add(new Employee 
            { 
                EmployeeNumber = "EMP-1001",
                FirstName = "Test",
                LastName = "Employee",
                Email = "test@example.com",
                JobTitle = "Developer",
                Salary = 50000,
                DepartmentId = 1,
                HireDate = DateTime.Now
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GenerateEmployeeNumberAsync();

            // Assert
            Assert.Equal("EMP-1002", result);
        }

        [Fact]
        public async Task GenerateEmployeeNumberAsync_WithMultipleEmployees_ReturnsCorrectNextNumber()
        {
            // Arrange
            var employees = new[]
            {
                new Employee { EmployeeNumber = "EMP-1001", FirstName = "Test1", LastName = "Employee1", Email = "test1@example.com", JobTitle = "Developer", Salary = 50000, DepartmentId = 1, HireDate = DateTime.Now },
                new Employee { EmployeeNumber = "EMP-1003", FirstName = "Test2", LastName = "Employee2", Email = "test2@example.com", JobTitle = "Developer", Salary = 50000, DepartmentId = 1, HireDate = DateTime.Now },
                new Employee { EmployeeNumber = "EMP-1005", FirstName = "Test3", LastName = "Employee3", Email = "test3@example.com", JobTitle = "Developer", Salary = 50000, DepartmentId = 1, HireDate = DateTime.Now }
            };

            _context.Employees.AddRange(employees);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GenerateEmployeeNumberAsync();

            // Assert
            Assert.Equal("EMP-1006", result); // Should be based on the highest existing number
        }

        [Fact]
        public async Task GenerateEmployeeNumberAsync_WithInvalidEmployeeNumber_ReturnsEMP1001()
        {
            // Arrange
            _context.Employees.Add(new Employee 
            { 
                EmployeeNumber = "INVALID-NUMBER",
                FirstName = "Test",
                LastName = "Employee",
                Email = "test@example.com",
                JobTitle = "Developer",
                Salary = 50000,
                DepartmentId = 1,
                HireDate = DateTime.Now
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GenerateEmployeeNumberAsync();

            // Assert
            Assert.Equal("EMP-1001", result); // Should fall back to default when number can't be parsed
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}