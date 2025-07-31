using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HCM_D.Pages.SalaryHistories
{
    [Authorize(Roles = "HR Admin,Manager")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;
        public List<SalaryHistory> SalaryHistories { get; set; } = new();
        public async Task OnGetAsync()
        {
            SalaryHistories = await _context.SalaryHistories.Include(s => s.Employee).ToListAsync();
        }
    }
}
