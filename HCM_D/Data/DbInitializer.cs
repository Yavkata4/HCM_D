using Microsoft.AspNetCore.Identity;
using HCM_D.Models;
using Microsoft.EntityFrameworkCore;
using HCM_D.Services;

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
            var employeeNumberService = scope.ServiceProvider.GetRequiredService<EmployeeNumberService>();

            // Ensure DB exists and run migrations
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

            // ✅ Seed default departments FIRST
            if (!context.Departments.Any())
            {
                context.Departments.AddRange(
                    new Department { Name = "Human Resources" },
                    new Department { Name = "Finance" },
                    new Department { Name = "Engineering" },
                    new Department { Name = "Marketing" },
                    new Department { Name = "IT Support" },
                    new Department { Name = "General" } // Default department for new employees
                );

                await context.SaveChangesAsync();
            }

            // Get default department for new employees
            var defaultDepartment = await context.Departments.FirstOrDefaultAsync(d => d.Name == "General") 
                ?? await context.Departments.FirstAsync();

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
                    
                    // Create Employee profile for admin - mark as admin
                    await CreateEmployeeProfileAsync(context, employeeNumberService, adminUser, defaultDepartment, "HR Administrator", 85000, isAdmin: true);
                }
            }
            else
            {
                // Update existing admin to be marked as admin (only if IsAdmin column exists)
                try
                {
                    var existingAdminEmployee = await context.Employees.FirstOrDefaultAsync(e => e.Email == adminEmail);
                    if (existingAdminEmployee != null)
                    {
                        existingAdminEmployee.IsAdmin = true;
                        await context.SaveChangesAsync();
                    }
                }
                catch (Exception)
                {
                    // IsAdmin column might not exist yet, ignore this error
                    // It will be handled after migration runs
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
                    
                    // Create Employee profile for manager - not admin
                    await CreateEmployeeProfileAsync(context, employeeNumberService, managerUser, defaultDepartment, "Department Manager", 75000, isAdmin: false);
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
                    
                    // Create Employee profile for employee - not admin
                    await CreateEmployeeProfileAsync(context, employeeNumberService, employeeUser, defaultDepartment, "Staff Employee", 55000, isAdmin: false);
                }
            }

            // ✅ Create Employee profiles for any existing Identity users that don't have them
            await CreateMissingEmployeeProfilesAsync(context, userManager, employeeNumberService, defaultDepartment);
        }

        private static async Task CreateEmployeeProfileAsync(
            ApplicationDbContext context, 
            EmployeeNumberService employeeNumberService, 
            IdentityUser user, 
            Department department, 
            string jobTitle, 
            decimal salary,
            bool isAdmin = false)
        {
            // Check if employee profile already exists
            var existingEmployee = await context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
            if (existingEmployee != null)
                return;

            // Extract first and last name from email or use defaults
            var emailPart = user.Email!.Split('@')[0];
            var nameParts = emailPart.Split('.');
            
            string firstName = nameParts.Length > 0 ? CapitalizeFirstLetter(nameParts[0]) : "Employee";
            string lastName = nameParts.Length > 1 ? CapitalizeFirstLetter(nameParts[1]) : "User";

            try
            {
                var employee = new Employee
                {
                    EmployeeNumber = await employeeNumberService.GenerateEmployeeNumberAsync(),
                    FirstName = firstName,
                    LastName = lastName,
                    Email = user.Email,
                    JobTitle = jobTitle,
                    Salary = salary,
                    DepartmentId = department.Id,
                    HireDate = DateTime.UtcNow,
                    IsAdmin = isAdmin
                };

                context.Employees.Add(employee);
                await context.SaveChangesAsync();
            }
            catch (Exception)
            {
                // If IsAdmin column doesn't exist yet, create without it
                // This will be updated after migration runs
                var employee = new Employee
                {
                    EmployeeNumber = await employeeNumberService.GenerateEmployeeNumberAsync(),
                    FirstName = firstName,
                    LastName = lastName,
                    Email = user.Email,
                    JobTitle = jobTitle,
                    Salary = salary,
                    DepartmentId = department.Id,
                    HireDate = DateTime.UtcNow
                    // IsAdmin will be added later via migration
                };

                context.Employees.Add(employee);
                await context.SaveChangesAsync();
            }
        }

        private static async Task CreateMissingEmployeeProfilesAsync(
            ApplicationDbContext context, 
            UserManager<IdentityUser> userManager, 
            EmployeeNumberService employeeNumberService, 
            Department defaultDepartment)
        {
            // Get all Identity users
            var allUsers = await userManager.Users.ToListAsync();
            
            foreach (var user in allUsers)
            {
                // Check if this user has an Employee profile
                var existingEmployee = await context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
                if (existingEmployee == null && !string.IsNullOrEmpty(user.Email))
                {
                    // Determine role and create appropriate profile
                    var roles = await userManager.GetRolesAsync(user);
                    var userRole = roles.FirstOrDefault() ?? "Employee";
                    
                    string jobTitle = userRole switch
                    {
                        "HR Admin" => "HR Administrator",
                        "Manager" => "Department Manager",
                        _ => "Staff Employee"
                    };
                    
                    decimal salary = userRole switch
                    {
                        "HR Admin" => 85000,
                        "Manager" => 75000,
                        _ => 55000
                    };

                    bool isAdmin = userRole == "HR Admin";

                    await CreateEmployeeProfileAsync(context, employeeNumberService, user, defaultDepartment, jobTitle, salary, isAdmin);
                }
            }
        }

        private static string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            
            return char.ToUpper(input[0]) + input[1..].ToLower();
        }

        public static void SeedEmployees(ApplicationDbContext context)
        {
            if (context.Employees.Any())
                return; // Already seeded

            var hrDepartment = context.Departments.FirstOrDefault(d => d.Name == "Human Resources");
            var itDepartment = context.Departments.FirstOrDefault(d => d.Name == "IT Support");

            if (hrDepartment == null || itDepartment == null)
                return; // Departments must be seeded first

            try
            {
                context.Employees.AddRange(
                    new Employee { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@company.com", JobTitle = "HR Specialist", Salary = 60000, DepartmentId = hrDepartment.Id, IsAdmin = false, EmployeeNumber = "EMP-2001", HireDate = DateTime.UtcNow },
                    new Employee { FirstName = "Bob", LastName = "Smith", Email = "bob.smith@company.com", JobTitle = "IT Specialist", Salary = 55000, DepartmentId = itDepartment.Id, IsAdmin = false, EmployeeNumber = "EMP-2002", HireDate = DateTime.UtcNow }
                );
            }
            catch (Exception)
            {
                // If IsAdmin column doesn't exist, create without it
                context.Employees.AddRange(
                    new Employee { FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@company.com", JobTitle = "HR Specialist", Salary = 60000, DepartmentId = hrDepartment.Id, EmployeeNumber = "EMP-2001", HireDate = DateTime.UtcNow },
                    new Employee { FirstName = "Bob", LastName = "Smith", Email = "bob.smith@company.com", JobTitle = "IT Specialist", Salary = 55000, DepartmentId = itDepartment.Id, EmployeeNumber = "EMP-2002", HireDate = DateTime.UtcNow }
                );
            }

            context.SaveChanges();
        }
    }
}
