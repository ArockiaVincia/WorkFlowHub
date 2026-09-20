using System.ComponentModel.DataAnnotations;

namespace EmployeeWorkFlowHub.Models
{
    /// <summary>
    /// Request DTO for creating or updating a Department.
    /// Excludes read-only audit/count fields like Id, EmployeeCount, and ProjectCount.
    /// </summary>
    public class DepartmentDTO
    {
        /// <summary>
        /// Gets or sets the official department name.
        /// </summary>
        [Required(ErrorMessage = "Department Name is required.")]
        [StringLength(50, ErrorMessage = "Department Name cannot exceed 50 characters.")]
        public string Name { get; set; } = string.Empty;
    }
}
