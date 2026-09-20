using EmployeeWorkFlowHub.Common.Helper;
using EmployeeWorkFlowHub.Models;
using EmployeeWorkFlowHub.Repository.Interfaces;
using EmployeeWorkFlowHub.Service.Interfaces;
using EmployeeWorkFlowHub.Services;

namespace EmployeeWorkFlowHub.Service.Services
{
    /// <summary>
    /// Implements IAuthenticateService for business logic and data orchestration.
    /// Handles user authentication, credential verification, and token coordination.
    /// </summary>
    public class AuthenticateService : IAuthenticateService
    {
        private readonly IAuthenticateRepository _authenticateRepository;
        private readonly ITokenService _tokenService;

        public AuthenticateService(IAuthenticateRepository authenticateRepository, ITokenService tokenService)
        {
            _authenticateRepository = authenticateRepository;
            _tokenService = tokenService;
        }

        public async Task<ResultArgs> AuthenticateUserAsync(LoginRequest request)
        {
            var result = new ResultArgs();

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                result.StatusCode = 400;
                result.StatusMessage = "Username and password are required.";
                return result;
            }

            var user = await _authenticateRepository.GetByUsernameAsync(request.Username.Trim());
            if (user == null)
            {
                result.StatusCode = 401;
                result.StatusMessage = "Invalid username or password.";
                return result;
            }

            if (!user.IsActive)
            {
                result.StatusCode = 401;
                result.StatusMessage = "Account is inactive. Please contact an administrator.";
                return result;
            }

            bool isValid = PasswordHasher.VerifyPassword(request.Password, user.PasswordHash);
            if (!isValid)
            {
                result.StatusCode = 401;
                result.StatusMessage = "Invalid username or password.";
                return result;
            }

            var (token, expiresAt) = _tokenService.GenerateToken(user);

            result.StatusCode = 200;
            result.StatusMessage = "Authentication successful.";
            result.ResultData = new LoginResponse
            {
                Token = token,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role,
                EmployeeId = user.EmployeeId,
                ExpiresAt = expiresAt
            };
            return result;
        }

        public async Task<ResultArgs> GetCurrentUserAsync(string username)
        {
            var result = new ResultArgs();
            if (string.IsNullOrWhiteSpace(username))
            {
                result.StatusCode = 401;
                result.StatusMessage = "Unauthenticated user.";
                return result;
            }

            var user = await _authenticateRepository.GetByUsernameAsync(username.Trim());
            if (user == null)
            {
                result.StatusCode = 404;
                result.StatusMessage = "User not found.";
                return result;
            }

            result.StatusCode = 200;
            result.StatusMessage = "User profile retrieved successfully.";
            result.ResultData = new
            {
                userId = user.Id,
                username = user.Username,
                fullName = user.FullName,
                role = user.Role,
                employeeId = user.EmployeeId ?? 0
            };
            return result;
        }
    }
}
