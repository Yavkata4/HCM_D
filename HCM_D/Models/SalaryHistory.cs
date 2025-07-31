using System.ComponentModel.DataAnnotations;

namespace HCM_D.Models
{
    public class SalaryHistory
    {
        public int Id { get; set; }

        [Required]
        public int EmployeeId { get; set; }

        [Required]
        public decimal OldSalary { get; set; }

        [Required]
        public decimal NewSalary { get; set; }

        public DateTime ChangedOn { get; set; } = DateTime.UtcNow;

        public string ChangedBy { get; set; } = string.Empty;

        public Employee? Employee { get; set; }
    }
}
