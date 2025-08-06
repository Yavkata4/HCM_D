using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Pages.Employees
{
    [Authorize(Roles = "HR Admin,Manager")] // Allow both HR Admin and Manager
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DeleteModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Employee Employee { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            Employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Employee == null) return NotFound();

            // Check permissions
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");

            if (!isHRAdmin)
            {
                if (isManager)
                {
                    var manager = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
                    if (manager == null || Employee.DepartmentId != manager.DepartmentId)
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

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();

            var employeeToDelete = await _context.Employees.FindAsync(id);
            if (employeeToDelete == null) return NotFound();

            // Verify permissions again
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");

            if (!isHRAdmin)
            {
                if (isManager)
                {
                    var manager = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
                    if (manager == null || employeeToDelete.DepartmentId != manager.DepartmentId)
                    {
                        return Forbid();
                    }
                }
                else
                {
                    return Forbid();
                }
            }

            _context.Employees.Remove(employeeToDelete);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Employee {employeeToDelete.FullName} has been deleted successfully.";

            return RedirectToPage("Index");
        }
    }
}
