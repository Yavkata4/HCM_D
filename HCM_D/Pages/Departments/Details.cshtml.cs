using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Pages.Departments
{
    [Authorize(Roles = "HR Admin,Manager")]
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DetailsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Department Department { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            Department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Department == null) return NotFound();

            // Check if current user has permission to view this department
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");

            // HR Admin can view any department
            if (!isHRAdmin)
            {
                // Managers can only view their own department
                if (isManager)
                {
                    var manager = await _context.Employees
                        .FirstOrDefaultAsync(e => e.Email == user.Email);
                    
                    if (manager == null || manager.DepartmentId != id)
                    {
                        return Forbid();
                    }
                }
                else
                {
                    return Forbid();
                }
            }

            return Page();
        }
    }
}
