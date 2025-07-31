using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace HCM_D.Pages.Employees
{
    [Authorize(Roles = "HR Admin,Manager,Employee")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DetailsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Employee Employee { get; set; }

        public List<SalaryHistory> SalaryHistoryList { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            Employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Employee == null)
                return NotFound();

            // Authorization logic
            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");
            var isEmployee = await _userManager.IsInRoleAsync(user, "Employee");

            if (isEmployee && Employee.Email != user.Email)
            {
                return Forbid(); // Employees can only view their own profile
            }
            if (isManager && !isHRAdmin)
            {
                var manager = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
                if (manager == null || Employee.DepartmentId != manager.DepartmentId)
                {
                    return Forbid(); // Managers can only view employees in their department
                }
            }
            // HR Admin can view any record

            SalaryHistoryList = await _context.SalaryHistories
                .Where(s => s.EmployeeId == id)
                .OrderByDescending(s => s.ChangedOn)
                .ToListAsync();

            return Page();
        }
    }
}
