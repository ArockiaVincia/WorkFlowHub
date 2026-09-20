using System.ComponentModel.DataAnnotations;

namespace EmployeeWorkFlowHub.Models
{
    /// <summary>
    /// Represents an Employee entity in the organization.
    /// </summary>
    public class Employee
    {
        /// <summary>
        /// Gets or sets the employee primary key ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique employee code (e.g. EMP-1001).
        /// </summary>
        [Required(ErrorMessage = "Employee Code is required.")]
        [StringLength(15, ErrorMessage = "Employee Code cannot exceed 15 characters.")]
        public string EmployeeCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full name of the employee.
        /// </summary>
        [Required(ErrorMessage = "Full Name is required.")]
        [StringLength(50, ErrorMessage = "Full Name cannot exceed 50 characters.")]
        public string FullName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the employee official email address.
        /// </summary>
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the foreign key department ID.
        /// </summary>
        [Required(ErrorMessage = "Department is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid department.")]
        public int DepartmentId { get; set; }

        /// <summary>
        /// Gets or sets the display name of the assigned department.
        /// </summary>
        public string? DepartmentName { get; set; }

        /// <summary>
        /// Gets or sets the job title or designation of the employee.
        /// </summary>
        [StringLength(50, ErrorMessage = "Designation cannot exceed 50 characters.")]
        public string? Designation { get; set; }

        /// <summary>
        /// Gets or sets whether the employee is active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Gets or sets the application role for user login credentials.
        /// </summary>
        public string? Role { get; set; }

        /// <summary>
        /// Gets or sets the plain text password when creating or updating login credentials.
        /// Never serialized in API responses for security.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public string? Password { get; set; }

        /// <summary>
        /// Gets or sets the hashed password stored in database.
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public string? PasswordHash { get; set; }

        /// <summary>
        /// Gets or sets the login username.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Gets or sets record creation date.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
