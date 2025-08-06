using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.ViewComponents
{
    public class UserDisplayNameViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public UserDisplayNameViewComponent(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var user = await _userManager.GetUserAsync(UserClaimsPrincipal);
            if (user?.Email == null)
            {
                return Content("User");
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == user.Email);
            
            var displayName = employee?.FullName ?? user.Email.Split('@')[0];
            
            return Content(displayName);
        }
    }
}