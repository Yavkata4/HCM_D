using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Pages.Employees
{
    [Authorize(Roles = "HR Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Employee Employee { get; set; }

        public SelectList Departments { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            Employee = await _context.Employees.FindAsync(id);

            if (Employee == null) return NotFound();

            Departments = new SelectList(_context.Departments, "Id", "Name", Employee.DepartmentId);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Departments = new SelectList(_context.Departments, "Id", "Name", Employee.DepartmentId);
                return Page();
            }

            var existingEmployee = await _context.Employees.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == Employee.Id);

            if (existingEmployee == null) return NotFound();

            // Check if salary has changed
            if (existingEmployee.Salary != Employee.Salary)
            {
                var salaryHistory = new SalaryHistory
                {
                    EmployeeId = Employee.Id,
                    OldSalary = existingEmployee.Salary,
                    NewSalary = Employee.Salary,
                    ChangedOn = DateTime.UtcNow
                };

                _context.SalaryHistories.Add(salaryHistory);
            }

            _context.Attach(Employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Employee updated.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Employees.Any(e => e.Id == Employee.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToPage("Index");
        }
    }
}
