using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HCM_D.Pages.Employees
{
    [Authorize(Roles = "HR Admin,Manager")] // Allow both HR Admin and Manager to edit
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<EditModel> _logger;

        public EditModel(ApplicationDbContext context, UserManager<IdentityUser> userManager, ILogger<EditModel> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public Employee Employee { get; set; } = new();

        public SelectList Departments { get; set; } = new SelectList(new List<Department>(), "Id", "Name");

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) 
            {
                _logger.LogWarning("Edit page accessed without employee ID");
                return NotFound();
            }

            Employee = await _context.Employees
                .AsNoTracking() // Don't track this entity
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (Employee == null) 
            {
                _logger.LogWarning("Employee with ID {EmployeeId} not found", id);
                return NotFound();
            }

            // Check if current user has permission to edit this employee
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");

            // HR Admin can edit anyone
            if (!isHRAdmin)
            {
                // Managers can only edit employees in their department
                if (isManager)
                {
                    var manager = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Email == user.Email);
                    if (manager == null || Employee.DepartmentId != manager.DepartmentId)
                    {
                        _logger.LogWarning("Manager {UserEmail} attempted to edit employee {EmployeeId} outside their department", user.Email, id);
                        return Forbid();
                    }
                }
                else
                {
                    _logger.LogWarning("User {UserEmail} without sufficient permissions attempted to edit employee {EmployeeId}", user.Email, id);
                    return Forbid();
                }
            }

            Departments = new SelectList(_context.Departments, "Id", "Name", Employee.DepartmentId);
            _logger.LogInformation("Edit page loaded for employee {EmployeeId} by user {UserEmail}", id, user.Email);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Edit form submitted for employee ID: {EmployeeId}", Employee.Id);

            // Reload departments for validation errors
            Departments = new SelectList(_context.Departments, "Id", "Name", Employee.DepartmentId);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("ModelState validation failed for employee {EmployeeId}. Errors: {Errors}", 
                    Employee.Id, 
                    string.Join(", ", ModelState.SelectMany(x => x.Value?.Errors?.Select(e => e.ErrorMessage) ?? new List<string>())));
                
                // Log each validation error for debugging
                foreach (var modelError in ModelState)
                {
                    foreach (var error in modelError.Value.Errors)
                    {
                        _logger.LogError("Validation Error for {Field}: {Error}", modelError.Key, error.ErrorMessage);
                    }
                }

                return Page();
            }

            var existingEmployee = await _context.Employees.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == Employee.Id);

            if (existingEmployee == null) 
            {
                _logger.LogError("Employee with ID {EmployeeId} not found during update", Employee.Id);
                return NotFound();
            }

            // Verify permission again for POST
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");

            if (!isHRAdmin)
            {
                if (isManager)
                {
                    var manager = await _context.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Email == user.Email);
                    if (manager == null || existingEmployee.DepartmentId != manager.DepartmentId)
                    {
                        _logger.LogWarning("Manager {UserEmail} attempted to save changes to employee {EmployeeId} outside their department", user.Email, Employee.Id);
                        return Forbid();
                    }
                }
                else
                {
                    _logger.LogWarning("User {UserEmail} without sufficient permissions attempted to save changes to employee {EmployeeId}", user.Email, Employee.Id);
                    return Forbid();
                }
            }

            // Check if salary has changed
            if (existingEmployee.Salary != Employee.Salary)
            {
                _logger.LogInformation("Salary change detected for employee {EmployeeId}: {OldSalary} -> {NewSalary}", 
                    Employee.Id, existingEmployee.Salary, Employee.Salary);

                // Get current user information
                var currentUser = await _userManager.GetUserAsync(User);
                var currentUserEmployee = await _context.Employees
                    .AsNoTracking() // Don't track this query
                    .FirstOrDefaultAsync(e => e.Email == currentUser.Email);
                
                string changedBy = "System";
                string changedByEmployeeId = "";
                string changedByFullName = "";
                string changedByEmail = "";

                if (currentUserEmployee != null)
                {
                    changedBy = $"{currentUserEmployee.FirstName} {currentUserEmployee.LastName}";
                    changedByEmployeeId = currentUserEmployee.EmployeeNumber;
                    changedByFullName = currentUserEmployee.FullName;
                    changedByEmail = currentUserEmployee.Email;
                }
                else if (currentUser != null)
                {
                    changedBy = currentUser.Email ?? "Unknown";
                    changedByEmail = currentUser.Email ?? "";
                    changedByFullName = currentUser.UserName ?? currentUser.Email ?? "Unknown";
                }

                var salaryHistory = new SalaryHistory
                {
                    EmployeeId = Employee.Id,
                    OldSalary = existingEmployee.Salary,
                    NewSalary = Employee.Salary,
                    ChangedOn = DateTime.UtcNow,
                    ChangedBy = changedBy,
                    ChangedByEmployeeId = changedByEmployeeId,
                    ChangedByFullName = changedByFullName,
                    ChangedByEmail = changedByEmail
                };

                _context.SalaryHistories.Add(salaryHistory);
            }

            // Use Update method instead of Attach to avoid tracking conflicts
            _context.Update(Employee);

            try
            {
                var rowsAffected = await _context.SaveChangesAsync();
                _logger.LogInformation("Employee {EmployeeId} updated successfully by {UserEmail}. Rows affected: {RowsAffected}", 
                    Employee.Id, user.Email, rowsAffected);
                
                TempData["Success"] = $"Employee {Employee.FullName} updated successfully.";
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency error updating employee {EmployeeId}", Employee.Id);
                
                if (!_context.Employees.Any(e => e.Id == Employee.Id))
                {
                    return NotFound();
                }
                else
                {
                    ModelState.AddModelError("", "The employee was modified by another user. Please reload and try again.");
                    return Page();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee {EmployeeId}", Employee.Id);
                ModelState.AddModelError("", "An error occurred while saving. Please try again.");
                return Page();
            }

            return RedirectToPage("Index");
        }
    }
}
