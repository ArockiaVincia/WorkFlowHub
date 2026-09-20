namespace EmployeeWorkFlowHub.Models
{
    /// <summary>
    /// Configuration options for JWT Token generation and validation.
    /// </summary>
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpiryMinutes { get; set; } = 120;
    }
}
