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
    [Authorize]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public EmployeesController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Get all employees (HR Admin and Manager only)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "HR Admin,Manager")]
        public async Task<ActionResult<IEnumerable<EmployeeDto>>> GetEmployees()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");

            IQueryable<Employee> query = _context.Employees.Include(e => e.Department);

            if (!isHRAdmin && isManager)
            {
                // Managers only see their department employees (excluding admins)
                var manager = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
                if (manager == null) return Forbid();
                
                query = query.Where(e => e.DepartmentId == manager.DepartmentId && !e.IsAdmin);
            }
            else if (isHRAdmin)
            {
                // HR Admin sees all employees
                query = query.Where(e => true);
            }

            var employees = await query
                .Select(e => new EmployeeDto
                {
                    Id = e.Id,
                    EmployeeNumber = e.EmployeeNumber,
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    Email = e.Email,
                    JobTitle = e.JobTitle,
                    Salary = e.Salary,
                    DepartmentName = e.Department!.Name,
                    HireDate = e.HireDate,
                    YearsOfService = e.YearsOfService
                })
                .ToListAsync();

            return Ok(employees);
        }

        /// <summary>
        /// Get employee by ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<EmployeeDto>> GetEmployee(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null) return NotFound();

            // Authorization check
            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");
            var isEmployee = await _userManager.IsInRoleAsync(user, "Employee");

            if (isEmployee && !isManager && !isHRAdmin)
            {
                // Employees can only access their own data
                if (employee.Email != user.Email)
                    return Forbid();
            }
            else if (isManager && !isHRAdmin)
            {
                // Managers can only access employees in their department
                var manager = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
                if (manager == null || employee.DepartmentId != manager.DepartmentId)
                    return Forbid();
            }

            var employeeDto = new EmployeeDto
            {
                Id = employee.Id,
                EmployeeNumber = employee.EmployeeNumber,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                JobTitle = employee.JobTitle,
                Salary = employee.Salary,
                DepartmentName = employee.Department!.Name,
                HireDate = employee.HireDate,
                YearsOfService = employee.YearsOfService
            };

            return Ok(employeeDto);
        }

        /// <summary>
        /// Create new employee (HR Admin and Manager only)
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "HR Admin,Manager")]
        public async Task<ActionResult<EmployeeDto>> CreateEmployee(CreateEmployeeDto createEmployeeDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var employee = new Employee
            {
                EmployeeNumber = createEmployeeDto.EmployeeNumber,
                FirstName = createEmployeeDto.FirstName,
                LastName = createEmployeeDto.LastName,
                Email = createEmployeeDto.Email,
                JobTitle = createEmployeeDto.JobTitle,
                Salary = createEmployeeDto.Salary,
                DepartmentId = createEmployeeDto.DepartmentId,
                HireDate = createEmployeeDto.HireDate,
                IsAdmin = false
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            var department = await _context.Departments.FindAsync(employee.DepartmentId);
            var employeeDto = new EmployeeDto
            {
                Id = employee.Id,
                EmployeeNumber = employee.EmployeeNumber,
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                JobTitle = employee.JobTitle,
                Salary = employee.Salary,
                DepartmentName = department?.Name ?? "",
                HireDate = employee.HireDate,
                YearsOfService = employee.YearsOfService
            };

            return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employeeDto);
        }

        /// <summary>
        /// Update employee (HR Admin and Manager only)
        /// </summary>
        [HttpPut("{id}")]
        [Authorize(Roles = "HR Admin,Manager")]
        public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDto updateEmployeeDto)
        {
            if (id != updateEmployeeDto.Id)
                return BadRequest();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound();

            // Authorization check for managers
            var user = await _userManager.GetUserAsync(User);
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");
            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");

            if (isManager && !isHRAdmin)
            {
                var manager = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user!.Email);
                if (manager == null || employee.DepartmentId != manager.DepartmentId)
                    return Forbid();
            }

            employee.FirstName = updateEmployeeDto.FirstName;
            employee.LastName = updateEmployeeDto.LastName;
            employee.Email = updateEmployeeDto.Email;
            employee.JobTitle = updateEmployeeDto.JobTitle;
            employee.Salary = updateEmployeeDto.Salary;
            employee.DepartmentId = updateEmployeeDto.DepartmentId;
            employee.HireDate = updateEmployeeDto.HireDate;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EmployeeExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Delete employee (HR Admin only)
        /// </summary>
        [HttpDelete("{id}")]
        [Authorize(Roles = "HR Admin")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null)
                return NotFound();

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Get employee statistics
        /// </summary>
        [HttpGet("statistics")]
        [Authorize(Roles = "HR Admin,Manager")]
        public async Task<ActionResult<EmployeeStatisticsDto>> GetEmployeeStatistics()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var isHRAdmin = await _userManager.IsInRoleAsync(user, "HR Admin");
            var isManager = await _userManager.IsInRoleAsync(user, "Manager");

            IQueryable<Employee> query = _context.Employees.Include(e => e.Department);

            if (!isHRAdmin && isManager)
            {
                var manager = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
                if (manager == null) return Forbid();
                query = query.Where(e => e.DepartmentId == manager.DepartmentId);
            }

            var employees = await query.ToListAsync();
            
            var statistics = new EmployeeStatisticsDto
            {
                TotalEmployees = employees.Count,
                AverageSalary = employees.Any() ? employees.Average(e => e.Salary) : 0,
                DepartmentBreakdown = employees
                    .GroupBy(e => e.Department!.Name)
                    .Select(g => new DepartmentStatsDto
                    {
                        DepartmentName = g.Key,
                        EmployeeCount = g.Count(),
                        AverageSalary = g.Average(e => e.Salary)
                    })
                    .ToList()
            };

            return Ok(statistics);
        }

        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.Id == id);
        }
    }

    // DTOs for API responses
    public class EmployeeDto
    {
        public int Id { get; set; }
        public string EmployeeNumber { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public decimal Salary { get; set; }
        public string DepartmentName { get; set; } = "";
        public DateTime HireDate { get; set; }
        public double YearsOfService { get; set; }
    }

    public class CreateEmployeeDto
    {
        public string EmployeeNumber { get; set; } = "";
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public decimal Salary { get; set; }
        public int DepartmentId { get; set; }
        public DateTime HireDate { get; set; } = DateTime.Today;
    }

    public class UpdateEmployeeDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Email { get; set; } = "";
        public string JobTitle { get; set; } = "";
        public decimal Salary { get; set; }
        public int DepartmentId { get; set; }
        public DateTime HireDate { get; set; }
    }

    public class EmployeeStatisticsDto
    {
        public int TotalEmployees { get; set; }
        public decimal AverageSalary { get; set; }
        public List<DepartmentStatsDto> DepartmentBreakdown { get; set; } = new();
    }

    public class DepartmentStatsDto
    {
        public string DepartmentName { get; set; } = "";
        public int EmployeeCount { get; set; }
        public decimal AverageSalary { get; set; }
    }
}