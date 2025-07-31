using System.ComponentModel.DataAnnotations;

namespace HCM_D.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        public string FirstName { get; set; } = string.Empty;
        [Required]
        public string LastName { get; set; } = string.Empty;
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string JobTitle { get; set; } = string.Empty;
        [Required]
        public decimal Salary { get; set; }
        [Required]
        public int DepartmentId { get; set; }
        public Department? Department { get; set; }
    }
}
