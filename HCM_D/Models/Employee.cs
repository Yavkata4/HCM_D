using System.ComponentModel.DataAnnotations;

namespace HCM_D.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Employee Number")]
        public string EmployeeNumber { get; set; } = string.Empty;

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [Display(Name = "Job Title")]
        public string JobTitle { get; set; } = string.Empty;
        
        [Required]
        public decimal Salary { get; set; }
        
        [Required]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }
        
        [Required]
        [Display(Name = "Hire Date")]
        public DateTime HireDate { get; set; } = DateTime.UtcNow;
        
        [Display(Name = "Is Admin/Boss")]
        public bool IsAdmin { get; set; } = false;
        
        public Department? Department { get; set; }

        [Display(Name = "Full Name")]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Years of Service")]
        public double YearsOfService => (DateTime.UtcNow - HireDate).TotalDays / 365.25;
    }
}
