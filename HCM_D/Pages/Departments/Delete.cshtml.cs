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
        public Department Department { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            Department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Department == null) return NotFound();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();

            var department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department != null)
            {
                if (department.Employees != null && department.Employees.Any())
                {
                    ModelState.AddModelError(string.Empty, "Cannot delete a department with employees.");
                    return Page();
                }

                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Department deleted.";
            }

            return RedirectToPage("Index");
        }
    }
}
