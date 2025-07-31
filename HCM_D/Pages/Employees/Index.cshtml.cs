using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Pages.Employees
{
    [Authorize(Roles = "HR Admin,Manager")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<Employee> Employees { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");

            var query = _context.Employees
                .Include(e => e.Department)
                .AsQueryable();

            if (!isHRAdmin)
            {
                var manager = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
                if (manager != null)
                {
                    query = query.Where(e => e.DepartmentId == manager.DepartmentId);
                }
            }

            Employees = await query.OrderBy(e => e.LastName).ToListAsync();
        }
    }
}
