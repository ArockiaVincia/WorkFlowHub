using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EmployeeWorkFlowHub.Models
{
    /// <summary>
    /// Represents an organizational Department entity within the Employee Workflow Hub.
    /// </summary>
    public class Department
    {
        /// <summary>
        /// Gets or sets the unique primary key identifier for the department.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the official department name.
        /// Must be unique across all departments and maximum 50 characters.
        /// </summary>
        [Required(ErrorMessage = "Department Name is required.")]
        [StringLength(50, ErrorMessage = "Department Name cannot exceed 50 characters.")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the count of active employees assigned to this department.
        /// Calculated dynamically via SQL Server stored procedure for listing views.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? EmployeeCount { get; set; }

        /// <summary>
        /// Gets or sets the count of active projects associated with this department.
        /// Calculated dynamically via SQL Server stored procedure for listing views.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public int? ProjectCount { get; set; }
    }
}
