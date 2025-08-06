using System.ComponentModel.DataAnnotations;

namespace HCM_D.Models
{
    public class DepartmentGrowth
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required]
        [Display(Name = "Year")]
        [Range(2020, 2030, ErrorMessage = "Year must be between 2020 and 2030")]
        public int Year { get; set; }

        [Required]
        [Display(Name = "Revenue")]
        [DataType(DataType.Currency)]
        public decimal Revenue { get; set; }

        [Required]
        [Display(Name = "Expenses")]
        [DataType(DataType.Currency)]
        public decimal Expenses { get; set; }

        [Display(Name = "Department Goal")]
        [DataType(DataType.Currency)]
        public decimal? DepartmentGoal { get; set; }

        [Display(Name = "Notes")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }

        [Display(Name = "Created On")]
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated On")]
        public DateTime UpdatedOn { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Department? Department { get; set; }

        // Calculated properties
        [Display(Name = "Net Profit")]
        public decimal NetProfit => Revenue - Expenses;

        [Display(Name = "Profit Margin")]
        public decimal ProfitMargin => Revenue > 0 ? (NetProfit / Revenue) * 100 : 0;

        [Display(Name = "Goal Achievement")]
        public decimal? GoalAchievement => DepartmentGoal.HasValue && DepartmentGoal > 0 
            ? (NetProfit / DepartmentGoal.Value) * 100 
            : null;
    }
}