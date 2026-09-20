using EmployeeWorkFlowHub.Models;

namespace EmployeeWorkFlowHub.Repository.Interfaces
{
    /// <summary>
    /// Repository interface for Task data operations.
    /// </summary>
    public interface ITaskRepository
    {
        Task<List<TaskItem>> GetAllAsync();
        Task<TaskItem?> GetByIdAsync(int id);
        Task<List<TaskItem>> GetByProjectLeadAsync(int leadEmployeeId);
        Task<List<TaskItem>> GetByEmployeeAsync(int employeeId);
        Task<List<TaskItem>> GetForQCAsync();
        Task<ResultArgs> InsertAsync(TaskItem task);
        Task<ResultArgs> UpdateAsync(TaskItem task);
        Task<ResultArgs> DeleteAsync(int id);
    }
}
