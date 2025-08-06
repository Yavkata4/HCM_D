using HCM_D.Data;
using HCM_D.Models;
using HCM_D.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HCM_D.Pages.Employees
{
    [Authorize(Roles = "HR Admin,Manager")] // Allow both HR Admin and Manager to create employees
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly EmployeeNumberService _employeeNumberService;

        public CreateModel(ApplicationDbContext context, EmployeeNumberService employeeNumberService)
        {
            _context = context;
            _employeeNumberService = employeeNumberService;
        }

        [BindProperty]
        public Employee Employee { get; set; } = new();

        public SelectList Departments { get; set; } = new SelectList(new List<Department>(), "Id", "Name");

        public async Task OnGetAsync()
        {
            // Generate a unique employee number when the page loads
            Employee.EmployeeNumber = await _employeeNumberService.GenerateEmployeeNumberAsync();
            Employee.HireDate = DateTime.Today; // Default to today's date
            
            Departments = new SelectList(_context.Departments, "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Ensure we have a unique employee number if not already set
            if (string.IsNullOrEmpty(Employee.EmployeeNumber))
            {
                Employee.EmployeeNumber = await _employeeNumberService.GenerateEmployeeNumberAsync();
            }

            if (!ModelState.IsValid)
            {
                Departments = new SelectList(_context.Departments, "Id", "Name", Employee.DepartmentId);
                return Page();
            }

            _context.Employees.Add(Employee);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Employee {Employee.FullName} (ID: {Employee.EmployeeNumber}) created successfully.";
            return RedirectToPage("Index");
        }
    }
}
