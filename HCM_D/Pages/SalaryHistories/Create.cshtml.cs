using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace HCM_D.Pages.SalaryHistories
{
    [Authorize(Roles = "HR Admin,Manager")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public CreateModel(ApplicationDbContext context) => _context = context;
        [BindProperty]
        public SalaryHistory SalaryHistory { get; set; } = new();
        public SelectList Employees { get; set; }
        public void OnGet()
        {
            Employees = new SelectList(_context.Employees.ToList(), "Id", "Email");
        }
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Employees = new SelectList(_context.Employees.ToList(), "Id", "Email");
                return Page();
            }
            _context.SalaryHistories.Add(SalaryHistory);
            await _context.SaveChangesAsync();
            return RedirectToPage("Index");
        }
    }
}
