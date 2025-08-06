using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HCM_D.Pages.Departments
{
    [Authorize(Roles = "HR Admin,Manager")]
    public class EmployeeStatisticsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        
        public EmployeeStatisticsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public int TotalEmployees { get; set; }
        public decimal AverageSalary { get; set; }
        public decimal HighestSalary { get; set; }
        public decimal LowestSalary { get; set; }
        public bool IsManager { get; set; }
        public string DepartmentName { get; set; } = "";

        public List<DepartmentCount> DepartmentCounts { get; set; } = new();
        public List<JobTitleCount> JobTitleCounts { get; set; } = new();

        public class DepartmentCount
        {
            public string Name { get; set; } = "";
            public int Count { get; set; }
        }
        public class JobTitleCount
        {
            public string Title { get; set; } = "";
            public int Count { get; set; }
        }

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;

            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");
            
            IsManager = isManager && !isHRAdmin;

            if (IsManager)
            {
                // Managers can only see employee statistics for their department
                var manager = await _context.Employees
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Email == user.Email);
                    
                if (manager != null && manager.Department != null)
                {
                    DepartmentName = manager.Department.Name;
                    
                    var employees = await _context.Employees
                        .Where(e => e.DepartmentId == manager.DepartmentId)
                        .Include(e => e.Department)
                        .ToListAsync();
                        
                    TotalEmployees = employees.Count;
                    AverageSalary = employees.Count > 0 ? employees.Average(e => e.Salary) : 0;
                    HighestSalary = employees.Count > 0 ? employees.Max(e => e.Salary) : 0;
                    LowestSalary = employees.Count > 0 ? employees.Min(e => e.Salary) : 0;

                    // For managers, department counts will only show their department
                    DepartmentCounts = new List<DepartmentCount>
                    {
                        new DepartmentCount { Name = manager.Department.Name, Count = employees.Count }
                    };

                    // Job title counts within their department
                    JobTitleCounts = employees
                        .GroupBy(e => e.JobTitle)
                        .Select(g => new JobTitleCount { Title = g.Key, Count = g.Count() })
                        .OrderByDescending(j => j.Count)
                        .ToList();
                }
            }
            else if (isHRAdmin)
            {
                // HR Admin can see all employee statistics
                var employees = await _context.Employees.Include(e => e.Department).ToListAsync();
                TotalEmployees = employees.Count;
                AverageSalary = employees.Count > 0 ? employees.Average(e => e.Salary) : 0;
                HighestSalary = employees.Count > 0 ? employees.Max(e => e.Salary) : 0;
                LowestSalary = employees.Count > 0 ? employees.Min(e => e.Salary) : 0;

                DepartmentCounts = employees
                    .GroupBy(e => e.Department?.Name ?? "No Department")
                    .Select(g => new DepartmentCount { Name = g.Key, Count = g.Count() })
                    .OrderByDescending(d => d.Count)
                    .ToList();

                JobTitleCounts = employees
                    .GroupBy(e => e.JobTitle)
                    .Select(g => new JobTitleCount { Title = g.Key, Count = g.Count() })
                    .OrderByDescending(j => j.Count)
                    .ToList();
            }
        }
    }
}
