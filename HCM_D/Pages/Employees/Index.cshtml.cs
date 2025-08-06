using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Pages.Employees
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public IList<Employee> Employees { get; set; } = new List<Employee>();
        public bool CanEditEmployees { get; set; }
        public bool IsEmployee { get; set; }
        public Employee? CurrentEmployee { get; set; }
        public bool IsManager { get; set; }
        public string DepartmentName { get; set; } = "";

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            // Check user role permissions
            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");
            var isEmployee = await _userManager.IsInRoleAsync(user, "Employee");
            
            IsEmployee = isEmployee && !isHRAdmin && !isManager;
            IsManager = isManager && !isHRAdmin;
            CanEditEmployees = isHRAdmin || isManager;

            if (IsEmployee)
            {
                // Regular employees can only see their own profile and colleagues in same department
                // Exclude admin/boss employees from regular employee view
                var employee = await _context.Employees
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Email == user.Email);
                    
                CurrentEmployee = employee;
                
                if (employee != null && employee.Department != null)
                {
                    DepartmentName = employee.Department.Name;
                    Employees = await _context.Employees
                        .Include(e => e.Department)
                        .Where(e => e.DepartmentId == employee.DepartmentId && !e.IsAdmin)
                        .OrderBy(e => e.FirstName)
                        .ThenBy(e => e.LastName)
                        .ToListAsync();
                }
            }
            else if (IsManager)
            {
                // Managers can see employees in their department (excluding admin/boss)
                var manager = await _context.Employees
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Email == user.Email);
                    
                if (manager != null && manager.Department != null)
                {
                    DepartmentName = manager.Department.Name;
                    Employees = await _context.Employees
                        .Include(e => e.Department)
                        .Where(e => e.DepartmentId == manager.DepartmentId && !e.IsAdmin)
                        .OrderBy(e => e.FirstName)
                        .ThenBy(e => e.LastName)
                        .ToListAsync();
                }
            }
            else if (isHRAdmin)
            {
                // HR Admin can see all employees (including admin/boss for management purposes)
                Employees = await _context.Employees
                    .Include(e => e.Department)
                    .OrderBy(e => e.FirstName)
                    .ThenBy(e => e.LastName)
                    .ToListAsync();
            }
        }
    }
}
