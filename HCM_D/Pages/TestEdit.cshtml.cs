using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Pages
{
    [Authorize(Roles = "HR Admin,Manager")]
    public class TestEditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public TestEditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Employee Employee { get; set; } = new();

        public SelectList Departments { get; set; } = new SelectList(new List<Department>(), "Id", "Name");

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return Page();
            }

            Employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (Employee != null)
            {
                Departments = new SelectList(_context.Departments, "Id", "Name", Employee.DepartmentId);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Departments = new SelectList(_context.Departments, "Id", "Name", Employee.DepartmentId);

            if (!ModelState.IsValid)
            {
                TempData["TestResult"] = $"? Form validation failed. Check the validation errors above.";
                return Page();
            }

            try
            {
                _context.Attach(Employee).State = EntityState.Modified;
                var rowsAffected = await _context.SaveChangesAsync();
                
                TempData["TestResult"] = $"? Employee saved successfully! Rows affected: {rowsAffected}";
                
                // Reload the employee to show updated data
                Employee = await _context.Employees
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Id == Employee.Id);
                
                return Page();
            }
            catch (Exception ex)
            {
                TempData["TestResult"] = $"? Error saving employee: {ex.Message}";
                return Page();
            }
        }
    }
}