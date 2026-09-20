using EmployeeWorkFlowHub.Models;

namespace EmployeeWorkFlowHub.Service.Interfaces
{
    /// <summary>
    /// Service interface for Task operations.
    /// </summary>
    public interface ITaskService
    {
        Task<ResultArgs> GetAllAsync(string userRole, int currentEmployeeId);
        Task<ResultArgs> GetByIdAsync(int id, string userRole, int currentEmployeeId);
        Task<ResultArgs> CreateAsync(TaskItem task, string userRole, int currentEmployeeId);
        Task<ResultArgs> UpdateAsync(TaskItem task, string userRole, int currentEmployeeId);
        Task<ResultArgs> UpdateStatusAsync(int id, string status, string userRole, int currentEmployeeId);
        Task<ResultArgs> DeleteAsync(int id, string userRole);
    }
}
