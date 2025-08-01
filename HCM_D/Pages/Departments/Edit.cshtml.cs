using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HCM_D.Pages.Departments
{
    [Authorize(Roles = "HR Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public EditModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public Department Department { get; set; } = null!;
        public List<Employee> AvailableEmployees { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            Department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (Department == null) return NotFound();

            var departmentEmployeeIds = Department.Employees?.Select(e => e.Id).ToList() ?? new List<int>();
            AvailableEmployees = await _context.Employees
                .Where(e => !departmentEmployeeIds.Contains(e.Id))
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var department = await _context.Departments.Include(d => d.Employees).FirstOrDefaultAsync(d => d.Id == Department.Id);
            if (department == null) return NotFound();

            department.Name = Department.Name;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Department updated.";
            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostRemoveEmployeeAsync(int departmentId, int removeEmployeeId)
        {
            var department = await _context.Departments.Include(d => d.Employees).FirstOrDefaultAsync(d => d.Id == departmentId);
            if (department == null) return NotFound();
            var employee = department.Employees?.FirstOrDefault(e => e.Id == removeEmployeeId);
            if (employee != null)
            {
                department.Employees?.Remove(employee);
                employee.DepartmentId = 0;
                await _context.SaveChangesAsync();
            }
            return RedirectToPage(new { id = departmentId });
        }

        public async Task<IActionResult> OnPostAddEmployeeAsync(int departmentId, int addEmployeeId)
        {
            var department = await _context.Departments.Include(d => d.Employees).FirstOrDefaultAsync(d => d.Id == departmentId);
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == addEmployeeId);
            if (department == null || employee == null) return NotFound();
            employee.DepartmentId = departmentId;
            department.Employees?.Add(employee);
            await _context.SaveChangesAsync();
            return RedirectToPage(new { id = departmentId });
        }
    }
}
