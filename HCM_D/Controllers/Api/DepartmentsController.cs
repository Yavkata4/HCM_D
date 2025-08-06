using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "HR Admin,Manager")]
    public class DepartmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public DepartmentsController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Get all departments with employee counts
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments()
        {
            var departments = await _context.Departments
                .Include(d => d.Employees)
                .Select(d => new DepartmentDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    EmployeeCount = d.Employees!.Count(e => !e.IsAdmin), // Exclude admin employees from count
                    AverageSalary = d.Employees!.Any() ? d.Employees.Where(e => !e.IsAdmin).Average(e => e.Salary) : 0
                })
                .ToListAsync();

            return Ok(departments);
        }

        /// <summary>
        /// Get department by ID with detailed information
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<DetailedDepartmentDto>> GetDepartment(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var isHRAdmin = await _userManager.IsInRoleAsync(user!, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user!, "Manager");

            // Filter employees based on role
            var employees = department.Employees ?? new List<Employee>();
            
            if (!isHRAdmin)
            {
                // Non-HR Admin users don't see admin employees
                employees = employees.Where(e => !e.IsAdmin).ToList();
                
                if (isManager)
                {
                    // Managers only see their own department
                    var manager = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);
                    if (manager == null || manager.DepartmentId != id)
                        return Forbid();
                }
            }

            var departmentDto = new DetailedDepartmentDto
            {
                Id = department.Id,
                Name = department.Name,
                EmployeeCount = employees.Count(),
                AverageSalary = employees.Any() ? employees.Average(e => e.Salary) : 0,
                TotalSalaryExpense = employees.Sum(e => e.Salary),
                Employees = employees.Select(e => new DepartmentEmployeeDto
                {
                    Id = e.Id,
                    EmployeeNumber = e.EmployeeNumber,
                    FullName = e.FullName,
                    JobTitle = e.JobTitle,
                    Salary = e.Salary,
                    HireDate = e.HireDate,
                    YearsOfService = e.YearsOfService
                }).OrderBy(e => e.FullName).ToList()
            };

            return Ok(departmentDto);
        }

        /// <summary>
        /// Create new department (HR Admin only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "HR Admin")]
        public async Task<ActionResult<DepartmentDto>> CreateDepartment(CreateDepartmentDto createDepartmentDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var department = new Department
            {
                Name = createDepartmentDto.Name
            };

            _context.Departments.Add(department);
            await _context.SaveChangesAsync();

            var departmentDto = new DepartmentDto
            {
                Id = department.Id,
                Name = department.Name,
                EmployeeCount = 0,
                AverageSalary = 0
            };

            return CreatedAtAction(nameof(GetDepartment), new { id = department.Id }, departmentDto);
        }

        /// <summary>
        /// Update department (HR Admin only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "HR Admin")]
        public async Task<IActionResult> UpdateDepartment(int id, UpdateDepartmentDto updateDepartmentDto)
        {
            if (id != updateDepartmentDto.Id)
                return BadRequest();

            var department = await _context.Departments.FindAsync(id);
            if (department == null)
                return NotFound();

            department.Name = updateDepartmentDto.Name;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DepartmentExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Delete department (HR Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "HR Admin")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            var department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                return NotFound();

            if (department.Employees != null && department.Employees.Any())
                return BadRequest("Cannot delete department with existing employees. Please reassign employees first.");

            _context.Departments.Remove(department);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Get department analytics
        /// </summary>
        [HttpGet("{id}/analytics")]
        public async Task<ActionResult<DepartmentAnalyticsDto>> GetDepartmentAnalytics(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var isHRAdmin = await _userManager.IsInRoleAsync(user!, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user!, "Manager");

            // Authorization check for managers
            if (!isHRAdmin && isManager)
            {
                var manager = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);
                if (manager == null || manager.DepartmentId != id)
                    return Forbid();
            }

            var department = await _context.Departments
                .Include(d => d.Employees)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
                return NotFound();

            var employees = department.Employees?.Where(e => !e.IsAdmin).ToList() ?? new List<Employee>();

            var analytics = new DepartmentAnalyticsDto
            {
                DepartmentId = id,
                DepartmentName = department.Name,
                TotalEmployees = employees.Count,
                AverageSalary = employees.Any() ? employees.Average(e => e.Salary) : 0,
                MedianSalary = employees.Any() ? CalculateMedian(employees.Select(e => e.Salary)) : 0,
                MinSalary = employees.Any() ? employees.Min(e => e.Salary) : 0,
                MaxSalary = employees.Any() ? employees.Max(e => e.Salary) : 0,
                TotalSalaryExpense = employees.Sum(e => e.Salary),
                AverageYearsOfService = employees.Any() ? employees.Average(e => e.YearsOfService) : 0,
                JobTitleDistribution = employees
                    .GroupBy(e => e.JobTitle)
                    .Select(g => new JobTitleDistributionDto
                    {
                        JobTitle = g.Key,
                        Count = g.Count(),
                        AverageSalary = g.Average(e => e.Salary)
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList()
            };

            return Ok(analytics);
        }

        private static decimal CalculateMedian(IEnumerable<decimal> values)
        {
            var sortedValues = values.OrderBy(x => x).ToList();
            var count = sortedValues.Count;
            
            if (count == 0) return 0;
            if (count % 2 == 0)
                return (sortedValues[count / 2 - 1] + sortedValues[count / 2]) / 2;
            else
                return sortedValues[count / 2];
        }

        private bool DepartmentExists(int id)
        {
            return _context.Departments.Any(e => e.Id == id);
        }
    }

    // DTOs for Departments API
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int EmployeeCount { get; set; }
        public decimal AverageSalary { get; set; }
    }

    public class DetailedDepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int EmployeeCount { get; set; }
        public decimal AverageSalary { get; set; }
        public decimal TotalSalaryExpense { get; set; }
        public List<DepartmentEmployeeDto> Employees { get; set; } = new();
    }

    public class DepartmentEmployeeDto
    {
        public int Id { get; set; }
        public string EmployeeNumber { get; set; } = "";
        public string FullName { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public decimal Salary { get; set; }
        public DateTime HireDate { get; set; }
        public double YearsOfService { get; set; }
    }

    public class CreateDepartmentDto
    {
        public string Name { get; set; } = "";
    }

    public class UpdateDepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class DepartmentAnalyticsDto
    {
        public int DepartmentId { get; set; }
        public string DepartmentName { get; set; } = "";
        public int TotalEmployees { get; set; }
        public decimal AverageSalary { get; set; }
        public decimal MedianSalary { get; set; }
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public decimal TotalSalaryExpense { get; set; }
        public double AverageYearsOfService { get; set; }
        public List<JobTitleDistributionDto> JobTitleDistribution { get; set; } = new();
    }

    public class JobTitleDistributionDto
    {
        public string JobTitle { get; set; } = "";
        public int Count { get; set; }
        public decimal AverageSalary { get; set; }
    }
}