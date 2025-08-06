using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Pages.SalaryHistories
{
    [Authorize(Roles = "Employee")]
    public class MyHistoryModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MyHistoryModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Employee? Employee { get; set; }
        public List<SalaryHistory> SalaryHistories { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            // Get the current employee's profile
            Employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Email == user.Email);

            if (Employee == null)
                return RedirectToPage("/Employees/MyProfile");

            // Get salary history for the current employee only
            SalaryHistories = await _context.SalaryHistories
                .Where(sh => sh.EmployeeId == Employee.Id)
                .OrderByDescending(sh => sh.ChangedOn)
                .ToListAsync();

            return Page();
        }
    }
}