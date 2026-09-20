using EmployeeWorkFlowHub.Models;

namespace EmployeeWorkFlowHub.Repository.Interfaces
{
    /// <summary>
    /// Repository interface for Project data operations.
    /// </summary>
    public interface IProjectRepository
    {
        Task<List<Project>> GetAllAsync();
        Task<Project?> GetByIdAsync(int id);
        Task<List<Project>> GetByManagerIdAsync(int managerId);
        Task<ResultArgs> InsertAsync(Project project);
        Task<ResultArgs> UpdateAsync(Project project);
        Task<ResultArgs> DeleteAsync(int id);
    }
}
