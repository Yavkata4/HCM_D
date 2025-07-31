using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Pages.Departments
{
    [Authorize(Roles = "HR Admin, Manager")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context) => _context = context;

        public IList<Department> Departments { get; set; } = [];

        public async Task OnGetAsync()
        {
            Departments = await _context.Departments
                .Include(d => d.Employees)
                .ToListAsync();
        }
    }
}
