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
    public class SalaryAnalyticsModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        
        public SalaryAnalyticsModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public int TotalEmployees { get; set; }
        public decimal TotalSalary { get; set; }
        public decimal AverageSalary { get; set; }
        public decimal HighestSalary { get; set; }
        public decimal LowestSalary { get; set; }
        public bool IsManager { get; set; }
        public string DepartmentName { get; set; } = "";

        public List<DepartmentSalaryStats> DepartmentAnalytics { get; set; } = new();

        public class DepartmentSalaryStats
        {
            public string Name { get; set; } = "";
            public int EmployeeCount { get; set; }
            public decimal TotalSalary { get; set; }
            public decimal AverageSalary { get; set; }
            public decimal HighestSalary { get; set; }
            public decimal LowestSalary { get; set; }
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
                // Managers can only see salary analytics for their department
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
                    TotalSalary = employees.Sum(e => e.Salary);
                    AverageSalary = employees.Count > 0 ? employees.Average(e => e.Salary) : 0;
                    HighestSalary = employees.Count > 0 ? employees.Max(e => e.Salary) : 0;
                    LowestSalary = employees.Count > 0 ? employees.Min(e => e.Salary) : 0;

                    var department = await _context.Departments
                        .Include(d => d.Employees)
                        .FirstOrDefaultAsync(d => d.Id == manager.DepartmentId);
                        
                    if (department != null)
                    {
                        var deptEmployees = department.Employees?.ToList() ?? new List<Employee>();
                        DepartmentAnalytics = new List<DepartmentSalaryStats>
                        {
                            new DepartmentSalaryStats
                            {
                                Name = department.Name,
                                EmployeeCount = deptEmployees.Count,
                                TotalSalary = deptEmployees.Sum(e => e.Salary),
                                AverageSalary = deptEmployees.Count > 0 ? deptEmployees.Average(e => e.Salary) : 0,
                                HighestSalary = deptEmployees.Count > 0 ? deptEmployees.Max(e => e.Salary) : 0,
                                LowestSalary = deptEmployees.Count > 0 ? deptEmployees.Min(e => e.Salary) : 0
                            }
                        };
                    }
                }
            }
            else if (isHRAdmin)
            {
                // HR Admin can see all salary analytics
                var employees = await _context.Employees
                    .Include(e => e.Department)
                    .ToListAsync();
                    
                TotalEmployees = employees.Count;
                TotalSalary = employees.Sum(e => e.Salary);
                AverageSalary = employees.Count > 0 ? employees.Average(e => e.Salary) : 0;
                HighestSalary = employees.Count > 0 ? employees.Max(e => e.Salary) : 0;
                LowestSalary = employees.Count > 0 ? employees.Min(e => e.Salary) : 0;

                var departments = await _context.Departments.Include(d => d.Employees).ToListAsync();
                DepartmentAnalytics = departments.Select(d => {
                    var deptEmployees = d.Employees?.ToList() ?? new List<Employee>();
                    return new DepartmentSalaryStats
                    {
                        Name = d.Name,
                        EmployeeCount = deptEmployees.Count,
                        TotalSalary = deptEmployees.Sum(e => e.Salary),
                        AverageSalary = deptEmployees.Count > 0 ? deptEmployees.Average(e => e.Salary) : 0,
                        HighestSalary = deptEmployees.Count > 0 ? deptEmployees.Max(e => e.Salary) : 0,
                        LowestSalary = deptEmployees.Count > 0 ? deptEmployees.Min(e => e.Salary) : 0
                    };
                }).ToList();
            }
        }
    }
}
