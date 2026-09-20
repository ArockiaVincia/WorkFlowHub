using System.ComponentModel.DataAnnotations;

namespace EmployeeWorkFlowHub.Models
{
    /// <summary>
    /// Request DTO for creating or updating a Project.
    /// Excludes database-generated Id and read-only DepartmentName/ProjectManagerName.
    /// </summary>
    public class ProjectDTO
    {
        [Required(ErrorMessage = "Project Name is required.")]
        [StringLength(50, ErrorMessage = "Project Name cannot exceed 50 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid department.")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Project Manager is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid project manager.")]
        public int? ProjectManagerId { get; set; }

        [Required(ErrorMessage = "Team Members are required.")]
        [StringLength(500, ErrorMessage = "Team Members cannot exceed 500 characters.")]
        public string? TeamMembers { get; set; }

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        public string Status { get; set; } = "In Progress";

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
