using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Pages.Departments
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Department Department { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            Department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Department == null) return NotFound();

            return Page();
        }
    }
}
