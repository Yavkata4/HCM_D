using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HCM_D.Pages.Departments
{
    [Authorize(Roles = "HR Admin,Manager")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        
        public EditModel(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public Department Department { get; set; } = null!;
        public List<Employee> AvailableEmployees { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            Department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (Department == null) return NotFound();

            // Check if current user has permission to edit this department
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");

            // HR Admin can edit any department
            if (!isHRAdmin)
            {
                // Managers can only edit their own department
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

            // Get employees not in current department - include those in "Unassigned" department
            var departmentEmployeeIds = Department.Employees?.Select(e => e.Id).ToList() ?? new List<int>();
            AvailableEmployees = await _context.Employees
                .Include(e => e.Department)
                .Where(e => !departmentEmployeeIds.Contains(e.Id))
                .OrderBy(e => e.Department!.Name == "Unassigned" ? 0 : 1) // Show Unassigned employees first
                .ThenBy(e => e.FirstName)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var department = await _context.Departments.Include(d => d.Employees).FirstOrDefaultAsync(d => d.Id == Department.Id);
            if (department == null) return NotFound();

            // Check permission again for POST
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");

            if (!isHRAdmin)
            {
                if (isManager)
                {
                    var manager = await _context.Employees
                        .FirstOrDefaultAsync(e => e.Email == user.Email);
                    
                    if (manager == null || manager.DepartmentId != Department.Id)
                    {
                        return Forbid();
                    }
                }
                else
                {
                    return Forbid();
                }
            }

            department.Name = Department.Name;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Department updated.";
            return RedirectToPage("Index");
        }

        public async Task<IActionResult> OnPostRemoveEmployeeAsync(int departmentId, int removeEmployeeId)
        {
            // Check permission for removing employees
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");

            if (!isHRAdmin)
            {
                if (isManager)
                {
                    var manager = await _context.Employees
                        .FirstOrDefaultAsync(e => e.Email == user.Email);
                    
                    if (manager == null || manager.DepartmentId != departmentId)
                    {
                        return Forbid();
                    }
                }
                else
                {
                    return Forbid();
                }
            }

            var department = await _context.Departments.Include(d => d.Employees).FirstOrDefaultAsync(d => d.Id == departmentId);
            if (department == null) return NotFound();
            
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == removeEmployeeId);
            if (employee != null)
            {
                // Find or create "Unassigned" department
                var unassignedDept = await _context.Departments
                    .FirstOrDefaultAsync(d => d.Name == "Unassigned");
                
                if (unassignedDept == null)
                {
                    // Create the Unassigned department if it doesn't exist
                    unassignedDept = new Department { Name = "Unassigned" };
                    _context.Departments.Add(unassignedDept);
                    await _context.SaveChangesAsync();
                }

                // Move employee to Unassigned department instead of setting DepartmentId to 0
                employee.DepartmentId = unassignedDept.Id;
                await _context.SaveChangesAsync();
                
                TempData["Success"] = $"Employee {employee.FirstName} {employee.LastName} has been moved to Unassigned department.";
            }
            
            return RedirectToPage(new { id = departmentId });
        }

        public async Task<IActionResult> OnPostAddEmployeeAsync(int departmentId, int addEmployeeId)
        {
            // Check permission for adding employees
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");

            if (!isHRAdmin)
            {
                if (isManager)
                {
                    var manager = await _context.Employees
                        .FirstOrDefaultAsync(e => e.Email == user.Email);
                    
                    if (manager == null || manager.DepartmentId != departmentId)
                    {
                        return Forbid();
                    }
                }
                else
                {
                    return Forbid();
                }
            }

            var department = await _context.Departments.Include(d => d.Employees).FirstOrDefaultAsync(d => d.Id == departmentId);
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Id == addEmployeeId);
            if (department == null || employee == null) return NotFound();
            
            employee.DepartmentId = departmentId;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Employee {employee.FirstName} {employee.LastName} has been added to {department.Name} department.";
            
            return RedirectToPage(new { id = departmentId });
        }
    }
}
