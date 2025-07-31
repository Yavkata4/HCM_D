using Microsoft.AspNetCore.Identity;
using HCM_D.Models;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Data
{
    public class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();

            // Ensure DB exists
            context.Database.Migrate();

            // Seed roles
            string[] roles = { "HR Admin", "Manager", "Employee" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Seed HR Admin
            string adminEmail = "admin@hr.com";
            string adminPassword = "Admin@123";

            var existingAdmin = await userManager.FindByEmailAsync(adminEmail);
            if (existingAdmin == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "HR Admin");
                }
            }

            // Seed Manager
            string managerEmail = "manager@company.com";
            string managerPassword = "Manager@123";
            var existingManager = await userManager.FindByEmailAsync(managerEmail);
            if (existingManager == null)
            {
                var managerUser = new IdentityUser
                {
                    UserName = managerEmail,
                    Email = managerEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(managerUser, managerPassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(managerUser, "Manager");
                }
            }

            // Seed Employee
            string employeeEmail = "employee@company.com";
            string employeePassword = "Employee@123";
            var existingEmployee = await userManager.FindByEmailAsync(employeeEmail);
            if (existingEmployee == null)
            {
                var employeeUser = new IdentityUser
                {
                    UserName = employeeEmail,
                    Email = employeeEmail,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(employeeUser, employeePassword);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(employeeUser, "Employee");
                }
            }

            // ✅ Seed default departments
            if (!context.Departments.Any())
            {
                context.Departments.AddRange(
                    new Department { Name = "Human Resources" },
                    new Department { Name = "Finance" },
                    new Department { Name = "Engineering" },
                    new Department { Name = "Marketing" },
                    new Department { Name = "IT Support" }
                );

                await context.SaveChangesAsync();
            }
        }
        public static void SeedEmployees(ApplicationDbContext context)
        {
            if (context.Employees.Any())
                return; // Already seeded

            var hrDepartment = context.Departments.FirstOrDefault(d => d.Name == "Human Resources");
            var itDepartment = context.Departments.FirstOrDefault(d => d.Name == "IT Support");

            if (hrDepartment == null || itDepartment == null)
                return; // Departments must be seeded first

            context.Employees.AddRange(
                new Employee { FirstName = "Alice", LastName = "Johnson", Salary = 60000, DepartmentId = hrDepartment.Id },
                new Employee { FirstName = "Bob", LastName = "Smith", Salary = 55000, DepartmentId = itDepartment.Id }
            );

            context.SaveChanges();
        }

    }
}
