using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Pages
{
    public class DebugModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DebugModel(
            ApplicationDbContext context, 
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public List<IdentityRole> AllRoles { get; set; } = new();
        public List<IdentityUser> AllUsers { get; set; } = new();

        public async Task OnGetAsync()
        {
            AllRoles = await _roleManager.Roles.ToListAsync();
            AllUsers = await _userManager.Users.ToListAsync();
        }

        public Employee? GetEmployeeForUser(string? email)
        {
            if (string.IsNullOrEmpty(email))
                return null;

            return _context.Employees
                .Include(e => e.Department)
                .FirstOrDefault(e => e.Email == email);
        }
    }
}