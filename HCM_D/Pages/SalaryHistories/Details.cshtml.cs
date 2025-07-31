using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace HCM_D.Pages.SalaryHistories
{
    [Authorize(Roles = "HR Admin,Manager")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public DetailsModel(ApplicationDbContext context) => _context = context;
        public SalaryHistory SalaryHistory { get; set; } = new();
        public async Task<IActionResult> OnGetAsync(int id)
        {
            SalaryHistory = await _context.SalaryHistories.Include(s => s.Employee).FirstOrDefaultAsync(s => s.Id == id);
            if (SalaryHistory == null) return NotFound();
            return Page();
        }
    }
}
