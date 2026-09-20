namespace EmployeeWorkFlowHub.Models
{
    /// <summary>
    /// Represents an authenticated system user.
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        [System.Text.Json.Serialization.JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty; // "Admin" or "User"
        public string FullName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int? EmployeeId { get; set; }
    }
}
