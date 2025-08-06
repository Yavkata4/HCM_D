using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HCM_D.Pages.SalaryHistories
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
        
        public List<EmployeeSalaryGroup> EmployeeSalaryHistories { get; set; } = new();
        public bool IsManager { get; set; }
        public string DepartmentName { get; set; } = "";
        
        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");
            
            IsManager = isManager && !isHRAdmin;

            if (IsManager)
            {
                // Managers can only see salary histories for employees in their department
                var manager = await _context.Employees
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Email == user.Email);
                    
                if (manager != null && manager.Department != null)
                {
                    DepartmentName = manager.Department.Name;
                    
                    var employeesWithHistory = await _context.Employees
                        .Include(e => e.Department)
                        .Where(e => e.DepartmentId == manager.DepartmentId && 
                                  _context.SalaryHistories.Any(sh => sh.EmployeeId == e.Id))
                        .Select(e => new EmployeeSalaryGroup
                        {
                            Employee = e,
                            LatestChange = _context.SalaryHistories
                                .Where(sh => sh.EmployeeId == e.Id)
                                .OrderByDescending(sh => sh.ChangedOn)
                                .First(),
                            TotalChanges = _context.SalaryHistories
                                .Count(sh => sh.EmployeeId == e.Id)
                        })
                        .ToListAsync();
                        
                    EmployeeSalaryHistories = employeesWithHistory;
                }
            }
            else if (isHRAdmin)
            {
                // HR Admin can see all salary histories
                var employeesWithHistory = await _context.Employees
                    .Include(e => e.Department)
                    .Where(e => _context.SalaryHistories.Any(sh => sh.EmployeeId == e.Id))
                    .Select(e => new EmployeeSalaryGroup
                    {
                        Employee = e,
                        LatestChange = _context.SalaryHistories
                            .Where(sh => sh.EmployeeId == e.Id)
                            .OrderByDescending(sh => sh.ChangedOn)
                            .First(),
                        TotalChanges = _context.SalaryHistories
                            .Count(sh => sh.EmployeeId == e.Id)
                    })
                    .ToListAsync();
                    
                EmployeeSalaryHistories = employeesWithHistory;
            }
        }
    }

    public class EmployeeSalaryGroup
    {
        public Employee Employee { get; set; } = new();
        public SalaryHistory LatestChange { get; set; } = new();
        public int TotalChanges { get; set; }
    }
}
