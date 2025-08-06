using HCM_D.Data;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.HealthChecks
{
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly ApplicationDbContext _context;

        public DatabaseHealthCheck(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Try to query the database
                await _context.Database.CanConnectAsync(cancellationToken);
                
                // Check if critical tables exist and have data
                var departmentCount = await _context.Departments.CountAsync(cancellationToken);
                var employeeCount = await _context.Employees.CountAsync(cancellationToken);

                var data = new Dictionary<string, object>
                {
                    { "departments", departmentCount },
                    { "employees", employeeCount },
                    { "database_connection", "healthy" }
                };

                return HealthCheckResult.Healthy("Database is healthy", data);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Database is unhealthy", ex);
            }
        }
    }

    public class EmployeeDataHealthCheck : IHealthCheck
    {
        private readonly ApplicationDbContext _context;

        public EmployeeDataHealthCheck(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                // Check for data integrity issues
                var employeesWithoutDepartments = await _context.Employees
                    .Where(e => e.DepartmentId <= 0)
                    .CountAsync(cancellationToken);

                var employeesWithoutNumbers = await _context.Employees
                    .Where(e => string.IsNullOrEmpty(e.EmployeeNumber))
                    .CountAsync(cancellationToken);

                var duplicateEmployeeNumbers = await _context.Employees
                    .GroupBy(e => e.EmployeeNumber)
                    .Where(g => g.Count() > 1)
                    .CountAsync(cancellationToken);

                var data = new Dictionary<string, object>
                {
                    { "employees_without_departments", employeesWithoutDepartments },
                    { "employees_without_numbers", employeesWithoutNumbers },
                    { "duplicate_employee_numbers", duplicateEmployeeNumbers }
                };

                if (employeesWithoutDepartments > 0 || employeesWithoutNumbers > 0 || duplicateEmployeeNumbers > 0)
                {
                    return HealthCheckResult.Degraded("Data integrity issues detected", null, data);
                }

                return HealthCheckResult.Healthy("Employee data integrity is good", data);
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Cannot check employee data integrity", ex);
            }
        }
    }
}