using System.ComponentModel.DataAnnotations;

namespace EmployeeWorkFlowHub.Models
{
    /// <summary>
    /// Request DTO for creating or updating an Employee.
    /// Excludes database-generated Id and read-only DepartmentName.
    /// </summary>
    public class EmployeeDTO
    {
        [Required(ErrorMessage = "Employee Code is required.")]
        [StringLength(15, ErrorMessage = "Employee Code cannot exceed 15 characters.")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(50, ErrorMessage = "Full Name cannot exceed 50 characters.")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid department.")]
        public int DepartmentId { get; set; }

        [StringLength(50, ErrorMessage = "Designation cannot exceed 50 characters.")]
        public string? Designation { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Role { get; set; }

        public string? Password { get; set; }

        public string? Username { get; set; }
    }
}
