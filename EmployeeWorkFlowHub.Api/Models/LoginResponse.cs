namespace EmployeeWorkFlowHub.Models
{
    /// <summary>
    /// Response payload returned upon successful authentication.
    /// </summary>
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? EmployeeId { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
