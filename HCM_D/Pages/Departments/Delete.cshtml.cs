using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Pages.Departments
{
    [Authorize(Roles = "HR Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public DeleteModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public Department Department { get; set; } = null!;
        public List<string> BlockingReasons { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            Department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Department == null) return NotFound();

            // Check for blocking relationships
            await CheckBlockingRelationships();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();

            // Don't use model validation for deletion - we only need the ID
            var department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null) return NotFound();

            // Set the Department for the view in case we need to return to the page
            Department = department;

            // Re-check blocking relationships for display
            await CheckBlockingRelationships();

            // Check if this is the "Unassigned" department
            if (department.Name == "Unassigned")
            {
                ModelState.AddModelError(string.Empty, "Cannot delete the 'Unassigned' department as it's used for employees not assigned to other departments.");
                return Page();
            }

            try
            {
                // First, move any remaining employees to "Unassigned" department
                if (department.Employees?.Any() == true)
                {
                    var unassignedDept = await GetOrCreateUnassignedDepartment();
                    foreach (var employee in department.Employees.ToList())
                    {
                        employee.DepartmentId = unassignedDept.Id;
                    }
                    await _context.SaveChangesAsync();
                }

                // Delete any DepartmentGrowth records associated with this department
                var departmentGrowthRecords = await _context.DepartmentGrowths
                    .Where(dg => dg.DepartmentId == department.Id)
                    .ToListAsync();

                if (departmentGrowthRecords.Any())
                {
                    _context.DepartmentGrowths.RemoveRange(departmentGrowthRecords);
                    await _context.SaveChangesAsync();
                }

                // Now delete the department
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Department '{department.Name}' has been deleted successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error deleting department: {ex.Message}";
                return Page();
            }

            return RedirectToPage("Index");
        }

        private async Task CheckBlockingRelationships()
        {
            BlockingReasons.Clear();

            if (Department?.Id == null) return;

            // Check for employees
            var employeeCount = await _context.Employees
                .CountAsync(e => e.DepartmentId == Department.Id);

            if (employeeCount > 0)
            {
                BlockingReasons.Add($"This department has {employeeCount} employee(s). Employees will be moved to 'Unassigned' department.");
            }

            // Check for department growth records
            var departmentGrowthCount = await _context.DepartmentGrowths
                .CountAsync(dg => dg.DepartmentId == Department.Id);

            if (departmentGrowthCount > 0)
            {
                BlockingReasons.Add($"This department has {departmentGrowthCount} department growth record(s). These will be deleted.");
            }

            // Check if this is the "Unassigned" department
            if (Department.Name == "Unassigned")
            {
                BlockingReasons.Add("Cannot delete the 'Unassigned' department as it's used for employees not assigned to other departments.");
            }
        }

        private async Task<Department> GetOrCreateUnassignedDepartment()
        {
            var unassignedDept = await _context.Departments
                .FirstOrDefaultAsync(d => d.Name == "Unassigned");

            if (unassignedDept == null)
            {
                unassignedDept = new Department { Name = "Unassigned" };
                _context.Departments.Add(unassignedDept);
                await _context.SaveChangesAsync();
            }

            return unassignedDept;
        }
    }
}
