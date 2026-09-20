using System.ComponentModel.DataAnnotations;

namespace EmployeeWorkFlowHub.Models
{
    /// <summary>
    /// Represents an organizational Project entity associated with a Department.
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Gets or sets the project primary key ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the project name.
        /// </summary>
        [Required(ErrorMessage = "Project Name is required.")]
        [StringLength(50, ErrorMessage = "Project Name cannot exceed 50 characters.")]
        public string Name { get; set; } = string.Empty;

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
        /// Gets or sets the foreign key for the Project Manager (Employee Id).
        /// </summary>
        [Required(ErrorMessage = "Project Manager is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid project manager.")]
        public int? ProjectManagerId { get; set; }

        /// <summary>
        /// Gets or sets the display full name of the assigned Project Manager.
        /// </summary>
        public string? ProjectManagerName { get; set; }

        /// <summary>
        /// Gets or sets the comma-separated or descriptive list of team members.
        /// </summary>
        [Required(ErrorMessage = "Team Members are required.")]
        [StringLength(500, ErrorMessage = "Team Members cannot exceed 500 characters.")]
        public string? TeamMembers { get; set; }

        /// <summary>
        /// Gets or sets the current project status (e.g. Planning, In Progress, Completed).
        /// </summary>
        [Required(ErrorMessage = "Status is required.")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        public string Status { get; set; } = "In Progress";

        /// <summary>
        /// Gets or sets the project start date.
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Gets or sets the project end date.
        /// </summary>
        public DateTime? EndDate { get; set; }
    }
}
