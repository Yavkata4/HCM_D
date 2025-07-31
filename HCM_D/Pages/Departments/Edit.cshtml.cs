using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Pages.Departments
{
    [Authorize(Roles = "HR Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public EditModel(ApplicationDbContext context) => _context = context;

        [BindProperty]
        public Department Department { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            Department = await _context.Departments.FindAsync(id);
            if (Department == null) return NotFound();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            _context.Attach(Department).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                TempData["Success"] = "Department updated.";
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Departments.Any(d => d.Id == Department.Id))
                    return NotFound();
                else throw;
            }

            return RedirectToPage("Index");
        }
    }
}
