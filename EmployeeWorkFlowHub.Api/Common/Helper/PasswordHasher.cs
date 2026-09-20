using System.Security.Cryptography;
using System.Text;

namespace EmployeeWorkFlowHub.Common.Helper
{
    /// <summary>
    /// Cryptographic helper for secure password hashing and verification using SHA-256.
    /// Matches SQL Server's HASHBYTES('SHA2_256', ...) format and existing user seeds.
    /// </summary>
    public static class PasswordHasher
    {
        /// <summary>
        /// Hashes the plain-text password using SHA-256 and returns a 64-character uppercase hexadecimal string.
        /// </summary>
        public static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                return string.Empty;

            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = SHA256.HashData(bytes);
            return Convert.ToHexString(hash);
        }

        /// <summary>
        /// Verifies whether the input plain-text password matches the stored SHA-256 hash.
        /// </summary>
        public static bool VerifyPassword(string password, string storedHash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(storedHash))
                return false;

            string computedHash = HashPassword(password);
            return string.Equals(computedHash, storedHash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
