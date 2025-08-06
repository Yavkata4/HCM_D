using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HCM_D.Pages
{
    public class AssignAdminRoleModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AssignAdminRoleModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public void OnGet()
        {
            // Just display the page
        }

        public async Task<IActionResult> OnPostAsync(string action)
        {
            if (action == "assign-admin")
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    TempData["Error"] = "User not found. Please sign in.";
                    return Page();
                }

                // Ensure HR Admin role exists
                if (!await _roleManager.RoleExistsAsync("HR Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("HR Admin"));
                }

                // Check if user already has the role
                if (await _userManager.IsInRoleAsync(user, "HR Admin"))
                {
                    TempData["Error"] = "You already have the HR Admin role.";
                    return Page();
                }

                // Assign the role
                var result = await _userManager.AddToRoleAsync(user, "HR Admin");
                if (result.Succeeded)
                {
                    TempData["Success"] = $"HR Admin role successfully assigned to {user.Email}. You can now edit employees!";
                }
                else
                {
                    TempData["Error"] = $"Failed to assign HR Admin role: {string.Join(", ", result.Errors.Select(e => e.Description))}";
                }
            }

            return Page();
        }
    }
}