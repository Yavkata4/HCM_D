using HCM_D.Data;
using HCM_D.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HCM_D.Pages.Departments
{
    [Authorize(Roles = "HR Admin,Manager")]
    public class ReportModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public ReportModel(ApplicationDbContext context) => _context = context;
        public List<Department> Departments { get; set; } = new();
        public async Task OnGetAsync()
        {
            Departments = await _context.Departments.Include(d => d.Employees).ToListAsync();
        }
    }
}
