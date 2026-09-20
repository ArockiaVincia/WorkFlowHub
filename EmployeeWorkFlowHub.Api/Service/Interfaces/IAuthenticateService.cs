using EmployeeWorkFlowHub.Models;

namespace EmployeeWorkFlowHub.Service.Interfaces
{
    /// <summary>
    /// Service interface for User Authentication for data access operations.
    /// </summary>
    public interface IAuthenticateService
    {
        Task<ResultArgs> AuthenticateUserAsync(LoginRequest request);
        Task<ResultArgs> GetCurrentUserAsync(string username);
    }
}
