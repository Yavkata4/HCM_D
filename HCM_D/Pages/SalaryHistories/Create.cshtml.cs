using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace HCM_D.Pages.SalaryHistories
{
    [Authorize(Roles = "HR Admin,Manager")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        
        public CreateModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        
        [BindProperty]
        public SalaryHistory SalaryHistory { get; set; } = new();
        public SelectList Employees { get; set; } = new SelectList(Enumerable.Empty<Employee>(), "Id", "Email");
        public string CurrentUserInfo { get; set; } = "";
        public bool IsManager { get; set; }
        public string DepartmentName { get; set; } = "";
        
        public async Task OnGetAsync()
        {
            await LoadEmployeesAsync();
            await LoadCurrentUserInfoAsync();
        }
        
        public async Task<IActionResult> OnGetEmployeeSalaryAsync(int employeeId)
        {
            // Check if user has permission to view this employee's salary
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return new JsonResult(new { salary = 0 });

            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");

            var employee = await _context.Employees.FindAsync(employeeId);
            if (employee == null) return new JsonResult(new { salary = 0 });

            // If manager, check if employee is in their department
            if (isManager && !isHRAdmin)
            {
                var manager = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
                if (manager == null || employee.DepartmentId != manager.DepartmentId)
                {
                    return new JsonResult(new { salary = 0 });
                }
            }

            return new JsonResult(new { salary = employee.Salary });
        }
        
        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                // Validate that an employee was selected
                if (SalaryHistory.EmployeeId <= 0)
                {
                    ModelState.AddModelError("SalaryHistory.EmployeeId", "Please select an employee.");
                }

                // Validate new salary
                if (SalaryHistory.NewSalary <= 0)
                {
                    ModelState.AddModelError("SalaryHistory.NewSalary", "New salary must be greater than 0.");
                }

                // Check if user has permission to modify this employee's salary
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
                    var isManager = await _userManager.IsInRoleAsync(user, "Manager");

                    if (isManager && !isHRAdmin)
                    {
                        var manager = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
                        var employee = await _context.Employees.FindAsync(SalaryHistory.EmployeeId);
                        
                        if (manager == null || employee == null || employee.DepartmentId != manager.DepartmentId)
                        {
                            ModelState.AddModelError("", "You can only modify salaries for employees in your department.");
                        }
                    }
                }

                if (!ModelState.IsValid)
                {
                    await LoadEmployeesAsync();
                    await LoadCurrentUserInfoAsync();
                    return Page();
                }

                // Get the current employee salary
                var currentEmployee = await _context.Employees.FindAsync(SalaryHistory.EmployeeId);
                if (currentEmployee == null)
                {
                    ModelState.AddModelError("SalaryHistory.EmployeeId", "Selected employee not found.");
                    await LoadEmployeesAsync();
                    await LoadCurrentUserInfoAsync();
                    return Page();
                }

                // Get current user info for ChangedBy fields
                var currentUser = await _userManager.GetUserAsync(User);
                var currentUserEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == currentUser!.Email);

                // Set the old salary and change tracking info
                SalaryHistory.OldSalary = currentEmployee.Salary;
                SalaryHistory.ChangedOn = DateTime.UtcNow;
                
                // Set ChangedBy information
                if (currentUserEmployee != null)
                {
                    SalaryHistory.ChangedBy = currentUserEmployee.FullName;
                    SalaryHistory.ChangedByEmployeeId = currentUserEmployee.EmployeeNumber;
                    SalaryHistory.ChangedByFullName = currentUserEmployee.FullName;
                    SalaryHistory.ChangedByEmail = currentUserEmployee.Email;
                }
                else
                {
                    SalaryHistory.ChangedBy = currentUser?.Email ?? "Unknown User";
                    SalaryHistory.ChangedByEmployeeId = "N/A";
                    SalaryHistory.ChangedByFullName = currentUser?.Email ?? "Unknown User";
                    SalaryHistory.ChangedByEmail = currentUser?.Email ?? "Unknown";
                }

                // Update the employee's current salary
                currentEmployee.Salary = SalaryHistory.NewSalary;

                // Add the salary history record
                _context.SalaryHistories.Add(SalaryHistory);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Salary change for {currentEmployee.FullName} has been recorded successfully.";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while saving the salary change: {ex.Message}");
                await LoadEmployeesAsync();
                await LoadCurrentUserInfoAsync();
                return Page();
            }
        }
        
        private async Task LoadEmployeesAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");
            
            IsManager = isManager && !isHRAdmin;

            List<Employee> availableEmployees;

            if (IsManager)
            {
                // Managers can only create salary changes for employees in their department
                var manager = await _context.Employees
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Email == user.Email);
                    
                if (manager != null && manager.Department != null)
                {
                    DepartmentName = manager.Department.Name;
                    availableEmployees = await _context.Employees
                        .Include(e => e.Department)
                        .Where(e => e.DepartmentId == manager.DepartmentId && !e.IsAdmin) // Exclude admin employees
                        .OrderBy(e => e.FirstName)
                        .ThenBy(e => e.LastName)
                        .ToListAsync();
                }
                else
                {
                    availableEmployees = new List<Employee>();
                }
            }
            else if (isHRAdmin)
            {
                // HR Admin can create salary changes for all employees
                availableEmployees = await _context.Employees
                    .Include(e => e.Department)
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                    .ToListAsync();
            }
            else
            {
                availableEmployees = new List<Employee>();
            }

            Employees = new SelectList(
                availableEmployees.Select(e => new {
                    e.Id,
                    DisplayText = $"{e.FirstName} {e.LastName} ({e.Email}) - {e.Department?.Name}"
                }),
                "Id",
                "DisplayText"
            );
        }
        
        private async Task LoadCurrentUserInfoAsync()
        {
            // Get current user info for display
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                var currentUserEmployee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.Email == currentUser.Email);

                if (currentUserEmployee != null)
                {
                    CurrentUserInfo = $"{currentUserEmployee.EmployeeNumber} - {currentUserEmployee.FullName} ({currentUserEmployee.Email})";
                }
                else
                {
                    CurrentUserInfo = currentUser.Email ?? "Unknown User";
                }
            }
            else
            {
                CurrentUserInfo = "Unknown User";
            }
        }
    }
}
