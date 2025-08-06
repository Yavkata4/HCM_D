using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Pages.CompanyGrowth
{
    [Authorize(Roles = "HR Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public class DepartmentGrowthSummary
        {
            public string DepartmentName { get; set; } = "";
            public int Year { get; set; }
            public decimal Revenue { get; set; }
            public decimal Expenses { get; set; }
            public decimal NetProfit => Revenue - Expenses;
            public decimal ProfitMargin => Revenue > 0 ? (NetProfit / Revenue) * 100 : 0;
            public decimal? DepartmentGoal { get; set; }
            public decimal? GoalAchievement => DepartmentGoal.HasValue && DepartmentGoal > 0 
                ? (NetProfit / DepartmentGoal.Value) * 100 
                : null;
        }

        public class CompanyOverview
        {
            public decimal TotalRevenue { get; set; }
            public decimal TotalExpenses { get; set; }
            public decimal TotalNetProfit => TotalRevenue - TotalExpenses;
            public decimal OverallProfitMargin => TotalRevenue > 0 ? (TotalNetProfit / TotalRevenue) * 100 : 0;
            public int NumberOfDepartments { get; set; }
            public int NumberOfEmployees { get; set; }
            public decimal AverageRevenuePerDepartment => NumberOfDepartments > 0 ? TotalRevenue / NumberOfDepartments : 0;
        }

        public List<DepartmentGrowthSummary> DepartmentGrowthData { get; set; } = new();
        public List<int> AvailableYears { get; set; } = new();
        public CompanyOverview CurrentYearOverview { get; set; } = new();
        public CompanyOverview PreviousYearOverview { get; set; } = new();
        public int SelectedYear { get; set; } = DateTime.Now.Year;

        public async Task OnGetAsync(int year = 0)
        {
            SelectedYear = year > 0 ? year : DateTime.Now.Year;

            // Get available years from department growth data
            AvailableYears = await _context.DepartmentGrowths
                .Select(dg => dg.Year)
                .Distinct()
                .OrderByDescending(y => y)
                .ToListAsync();

            if (!AvailableYears.Any())
            {
                // Add current year if no data exists
                AvailableYears.Add(DateTime.Now.Year);
            }

            // Get department growth data for selected year
            DepartmentGrowthData = await _context.DepartmentGrowths
                .Include(dg => dg.Department)
                .Where(dg => dg.Year == SelectedYear)
                .Select(dg => new DepartmentGrowthSummary
                {
                    DepartmentName = dg.Department!.Name,
                    Year = dg.Year,
                    Revenue = dg.Revenue,
                    Expenses = dg.Expenses,
                    DepartmentGoal = dg.DepartmentGoal
                })
                .OrderBy(d => d.DepartmentName)
                .ToListAsync();

            // Calculate current year overview
            CurrentYearOverview = await CalculateCompanyOverview(SelectedYear);

            // Calculate previous year overview for comparison
            if (SelectedYear > 2020)
            {
                PreviousYearOverview = await CalculateCompanyOverview(SelectedYear - 1);
            }
        }

        private async Task<CompanyOverview> CalculateCompanyOverview(int year)
        {
            var yearData = await _context.DepartmentGrowths
                .Where(dg => dg.Year == year)
                .ToListAsync();

            var totalEmployees = await _context.Employees
                .Where(e => !e.IsAdmin) // Exclude admin/boss from employee count
                .CountAsync();

            var totalDepartments = await _context.Departments.CountAsync();

            return new CompanyOverview
            {
                TotalRevenue = yearData.Sum(d => d.Revenue),
                TotalExpenses = yearData.Sum(d => d.Expenses),
                NumberOfDepartments = totalDepartments,
                NumberOfEmployees = totalEmployees
            };
        }

        public async Task<IActionResult> OnPostCreateSampleDataAsync()
        {
            // Create sample data for demonstration
            var currentYear = DateTime.Now.Year;
            var departments = await _context.Departments.ToListAsync();

            foreach (var dept in departments)
            {
                // Check if data already exists
                var existingData = await _context.DepartmentGrowths
                    .FirstOrDefaultAsync(dg => dg.DepartmentId == dept.Id && dg.Year == currentYear);

                if (existingData == null)
                {
                    var random = new Random();
                    var baseRevenue = random.Next(100000, 500000);
                    var expenses = baseRevenue * (decimal)(0.6 + random.NextDouble() * 0.2); // 60-80% of revenue

                    var departmentGrowth = new DepartmentGrowth
                    {
                        DepartmentId = dept.Id,
                        Year = currentYear,
                        Revenue = baseRevenue,
                        Expenses = expenses,
                        DepartmentGoal = baseRevenue * 0.25m, // 25% profit margin goal
                        Notes = $"Sample data for {dept.Name} department",
                        CreatedOn = DateTime.UtcNow,
                        UpdatedOn = DateTime.UtcNow
                    };

                    _context.DepartmentGrowths.Add(departmentGrowth);
                }
            }

            await _context.SaveChangesAsync();
            TempData["Success"] = "Sample growth data has been created successfully!";
            return RedirectToPage();
        }
    }
}