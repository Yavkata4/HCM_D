using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Pages.Employees
{
    [Authorize(Roles = "HR Admin,Manager")] // Removed Employee access
    public class MyDepartmentModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public MyDepartmentModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Employee? Employee { get; set; }
        public List<Employee> DepartmentColleagues { get; set; } = new();
        public int DepartmentEmployeeCount { get; set; }
        public decimal DepartmentAverageSalary { get; set; }
        public int UniqueJobTitles { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            // Only HR Admin and Manager can access department information
            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");

            if (!isHRAdmin && !isManager)
            {
                return Forbid(); // Employees cannot access this page
            }

            // Get the current user's employee profile
            Employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Email == user.Email);

            if (Employee?.Department == null)
                return RedirectToPage("/Index");

            // Get department colleagues
            DepartmentColleagues = await _context.Employees
                .Where(e => e.DepartmentId == Employee.DepartmentId)
                .OrderBy(e => e.FirstName)
                .ThenBy(e => e.LastName)
                .ToListAsync();

            // Calculate department statistics
            DepartmentEmployeeCount = DepartmentColleagues.Count;

            if (DepartmentColleagues.Any())
            {
                DepartmentAverageSalary = DepartmentColleagues.Average(e => e.Salary);
                UniqueJobTitles = DepartmentColleagues.Select(e => e.JobTitle).Distinct().Count();
            }

            return Page();
        }
    }
}