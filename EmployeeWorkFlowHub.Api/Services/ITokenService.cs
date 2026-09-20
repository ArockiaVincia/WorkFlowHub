using EmployeeWorkFlowHub.Models;

namespace EmployeeWorkFlowHub.Services
{
    /// <summary>
    /// Service contract for generating JSON Web Tokens (JWT).
    /// </summary>
    public interface ITokenService
    {
        /// <summary>
        /// Generates a signed JWT security token containing claims for the authenticated user.
        /// </summary>
        /// <param name="user">The authenticated user entity.</param>
        /// <returns>A tuple with the serialized JWT token and expiration timestamp.</returns>
        (string Token, DateTime ExpiresAt) GenerateToken(User user);
    }
}
