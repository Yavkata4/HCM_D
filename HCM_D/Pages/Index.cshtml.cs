using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public Employee? CurrentEmployee { get; set; }
        public string DisplayName { get; set; } = "Employee";

        public async Task OnGetAsync()
        {
            // Get the current user's employee profile for personalized welcome message
            var user = await _userManager.GetUserAsync(User);
            if (user?.Email != null)
            {
                CurrentEmployee = await _context.Employees
                    .Include(e => e.Department)
                    .FirstOrDefaultAsync(e => e.Email == user.Email);
                
                if (CurrentEmployee != null)
                {
                    DisplayName = CurrentEmployee.FirstName;
                }
                else
                {
                    // Fallback to email username if no employee profile found
                    DisplayName = user.Email.Split('@')[0];
                }
            }
        }
    }
}
