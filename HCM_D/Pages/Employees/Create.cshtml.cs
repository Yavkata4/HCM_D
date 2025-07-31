using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HCM_D.Pages.Employees
{
    [Authorize(Roles = "HR Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Employee Employee { get; set; }

        public SelectList Departments { get; set; }

        public void OnGet()
        {
            Departments = new SelectList(_context.Departments, "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Departments = new SelectList(_context.Departments, "Id", "Name", Employee.DepartmentId);
                return Page();
            }

            _context.Employees.Add(Employee);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Employee created.";
            return RedirectToPage("Index");
        }
    }
}
