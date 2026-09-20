using System.ComponentModel.DataAnnotations;

namespace EmployeeWorkFlowHub.Models
{
    /// <summary>
    /// Represents an actionable Task entity assigned to an Employee.
    /// Named TaskItem to avoid collision with System.Threading.Tasks.Task.
    /// </summary>
    public class TaskItem
    {
        /// <summary>
        /// Gets or sets the task primary key ID.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the task title.
        /// </summary>
        [Required(ErrorMessage = "Task Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the detailed task description.
        /// </summary>
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the associated project primary key ID.
        /// </summary>
        [Required(ErrorMessage = "Project is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid project.")]
        public int? ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the display name of the associated project.
        /// </summary>
        public string? ProjectName { get; set; }

        /// <summary>
        /// Gets or sets the assigned employee primary key ID.
        /// </summary>
        [Required(ErrorMessage = "Assigned Employee is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid employee.")]
        public int EmployeeId { get; set; }

        /// <summary>
        /// Gets or sets the display name of the assigned employee.
        /// </summary>
        public string? EmployeeName { get; set; }

        /// <summary>
        /// Gets or sets the priority level (e.g. High, Medium, Low).
        /// </summary>
        [Required(ErrorMessage = "Priority is required.")]
        [StringLength(50, ErrorMessage = "Priority cannot exceed 50 characters.")]
        public string Priority { get; set; } = "Medium";

        /// <summary>
        /// Gets or sets the deadline date for the task.
        /// </summary>
        [Required(ErrorMessage = "Due Date is required.")]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);

        /// <summary>
        /// Gets or sets the current workflow progress status ('To Do', 'In Progress', 'Completed').
        /// </summary>
        [Required(ErrorMessage = "Status is required.")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        public string Status { get; set; } = "To Do";
    }

    /// <summary>
    /// Request DTO for updating task workflow status.
    /// </summary>
    public class UpdateStatusRequest
    {
        [Required(ErrorMessage = "Status is required.")]
        public string Status { get; set; } = string.Empty;
    }
}
