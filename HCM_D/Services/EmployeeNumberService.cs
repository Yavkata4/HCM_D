using HCM_D.Data;
using HCM_D.Models;
using Microsoft.EntityFrameworkCore;

namespace HCM_D.Services
{
    public class EmployeeNumberService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeNumberService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateEmployeeNumberAsync()
        {
            // Get the last employee number
            var lastEmployee = await _context.Employees
                .OrderByDescending(e => e.Id)
                .FirstOrDefaultAsync();

            int nextNumber = 1001; // Starting number

            if (lastEmployee != null && !string.IsNullOrEmpty(lastEmployee.EmployeeNumber))
            {
                // Extract number from last employee number (e.g., "EMP-1001" -> 1001)
                var numberPart = lastEmployee.EmployeeNumber.Split('-').LastOrDefault();
                if (int.TryParse(numberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"EMP-{nextNumber:D4}";
        }
    }
}