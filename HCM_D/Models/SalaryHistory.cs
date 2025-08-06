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

        [Display(Name = "Changed By")]
        public string ChangedBy { get; set; } = string.Empty;

        [Display(Name = "Changed By Employee ID")]
        public string ChangedByEmployeeId { get; set; } = string.Empty;

        [Display(Name = "Changed By Full Name")]
        public string ChangedByFullName { get; set; } = string.Empty;

        [Display(Name = "Changed By Email")]
        public string ChangedByEmail { get; set; } = string.Empty;

        public Employee? Employee { get; set; }

        [Display(Name = "Salary Change")]
        public decimal SalaryChange => NewSalary - OldSalary;

        [Display(Name = "Change Percentage")]
        public decimal ChangePercentage => OldSalary > 0 ? ((NewSalary - OldSalary) / OldSalary) * 100 : 0;
    }
}
