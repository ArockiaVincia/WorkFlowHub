namespace EmployeeWorkFlowHub.Models
{
    /// <summary>
    /// Response DTO for created or updated Department.
    /// Excludes employeeCount and projectCount.
    /// </summary>
    public class DepartmentResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
