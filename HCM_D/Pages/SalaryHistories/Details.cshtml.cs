using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HCM_D.Pages.SalaryHistories
{
    [Authorize(Roles = "HR Admin,Manager")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public DetailsModel(ApplicationDbContext context) => _context = context;
        
        public Employee Employee { get; set; } = new();
        public List<SalaryHistory> SalaryHistories { get; set; } = new();
        
        public async Task<IActionResult> OnGetAsync(int employeeId)
        {
            Employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == employeeId);
                
            if (Employee == null) return NotFound();
            
            SalaryHistories = await _context.SalaryHistories
                .Where(sh => sh.EmployeeId == employeeId)
                .OrderByDescending(sh => sh.ChangedOn)
                .ToListAsync();
                
            return Page();
        }
    }
}
