using System.ComponentModel.DataAnnotations;

namespace EmployeeWorkFlowHub.Models
{
    /// <summary>
    /// Request DTO for creating or updating a Task.
    /// Excludes database-generated Id and read-only ProjectName/EmployeeName.
    /// </summary>
    public class TaskDTO
    {
        [Required(ErrorMessage = "Task Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Project is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid project.")]
        public int? ProjectId { get; set; }

        [Required(ErrorMessage = "Assigned Employee is required.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid employee.")]
        public int EmployeeId { get; set; }

        [Required(ErrorMessage = "Priority is required.")]
        [StringLength(50, ErrorMessage = "Priority cannot exceed 50 characters.")]
        public string Priority { get; set; } = "Medium";

        [Required(ErrorMessage = "Due Date is required.")]
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(7);

        [Required(ErrorMessage = "Status is required.")]
        [StringLength(50, ErrorMessage = "Status cannot exceed 50 characters.")]
        public string Status { get; set; } = "To Do";
    }
}
