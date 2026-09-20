using EmployeeWorkFlowHub.Models;

namespace EmployeeWorkFlowHub.Service.Interfaces
{
    /// <summary>
    /// Service interface for Project operations.
    /// </summary>
    public interface IProjectService
    {
        Task<ResultArgs> GetAllAsync(string userRole, int currentEmployeeId);
        Task<ResultArgs> GetByIdAsync(int id, string userRole, int currentEmployeeId);
        Task<ResultArgs> CreateAsync(Project project, string userRole, int currentEmployeeId);
        Task<ResultArgs> UpdateAsync(Project project, string userRole, int currentEmployeeId);
        Task<ResultArgs> DeleteAsync(int id, string userRole, int currentEmployeeId);
    }
}
