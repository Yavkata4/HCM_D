using HCM_D.Data;
using HCM_D.Models;
using HCM_D.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Pages.Employees
{
    [Authorize(Roles = "Employee,Manager,HR Admin")]
    public class MyProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly EmployeeNumberService _employeeNumberService;

        public MyProfileModel(ApplicationDbContext context, UserManager<IdentityUser> userManager, EmployeeNumberService employeeNumberService)
        {
            _context = context;
            _userManager = userManager;
            _employeeNumberService = employeeNumberService;
        }

        public Employee? Employee { get; set; }
        public List<SalaryHistory> SalaryHistories { get; set; } = new();
        public int DepartmentEmployeeCount { get; set; }
        public decimal DepartmentAverageSalary { get; set; }
        public string UserRole { get; set; } = "Employee";
        public bool ProfileWasJustCreated { get; set; } = false;

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            // Get employee profile
            Employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Email == user.Email);

            // If no employee profile exists, create one automatically
            if (Employee == null && !string.IsNullOrEmpty(user.Email))
            {
                await CreateEmployeeProfileForUserAsync(user.Email);
                
                // Fetch the newly created profile
                Employee = await _context.Employees
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Email == user.Email);
                
                ProfileWasJustCreated = true;
            }

            if (Employee != null)
            {
                // Get salary history
                SalaryHistories = await _context.SalaryHistories
                    .Where(sh => sh.EmployeeId == Employee.Id)
                    .OrderByDescending(sh => sh.ChangedOn)
                    .Take(5)
                    .ToListAsync();

                // Get department statistics
                if (Employee.Department != null)
                {
                    DepartmentEmployeeCount = await _context.Employees
                        .CountAsync(e => e.DepartmentId == Employee.DepartmentId);

                    var departmentSalaries = await _context.Employees
                        .Where(e => e.DepartmentId == Employee.DepartmentId)
                        .Select(e => e.Salary)
                        .ToListAsync();

                    DepartmentAverageSalary = departmentSalaries.Any() ? departmentSalaries.Average() : 0;
                }

                // Get user role
                var roles = await _userManager.GetRolesAsync(user);
                UserRole = roles.FirstOrDefault() ?? "Employee";
            }

            return Page();
        }

        private async Task CreateEmployeeProfileForUserAsync(string userEmail)
        {
            // Check if employee profile already exists
            var existingEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == userEmail);
            if (existingEmployee != null)
                return;

            // Get default department
            var defaultDepartment = await _context.Departments.FirstOrDefaultAsync(d => d.Name == "General") 
                ?? await _context.Departments.FirstAsync();

            // Extract first and last name from email
            var emailPart = userEmail.Split('@')[0];
            var nameParts = emailPart.Split('.');
            
            string firstName = nameParts.Length > 0 ? CapitalizeFirstLetter(nameParts[0]) : "New";
            string lastName = nameParts.Length > 1 ? CapitalizeFirstLetter(nameParts[1]) : "Employee";

            var employee = new Employee
            {
                EmployeeNumber = await _employeeNumberService.GenerateEmployeeNumberAsync(),
                FirstName = firstName,
                LastName = lastName,
                Email = userEmail,
                JobTitle = "Staff Employee",
                Salary = 50000, // Default starting salary
                DepartmentId = defaultDepartment.Id,
                HireDate = DateTime.UtcNow
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        private static string CapitalizeFirstLetter(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            
            return char.ToUpper(input[0]) + input[1..].ToLower();
        }
    }
}
