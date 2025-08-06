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
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        
        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Department> Departments { get; set; } = [];
        public bool IsManager { get; set; }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");
            
            IsManager = isManager && !isHRAdmin;

            if (IsManager)
            {
                // Managers can only see their own department
                var manager = await _context.Employees
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Email == user.Email);
                    
                if (manager != null && manager.Department != null)
                {
                    var department = await _context.Departments
                        .Include(d => d.Employees)
                        .FirstOrDefaultAsync(d => d.Id == manager.DepartmentId);
                        
                    if (department != null)
                    {
                        Departments = new List<Department> { department };
                    }
                }
            }
            else if (isHRAdmin)
            {
                // HR Admin can see all departments
                Departments = await _context.Departments
                    .Include(d => d.Employees)
                    .ToListAsync();
            }
        }
    }
}
