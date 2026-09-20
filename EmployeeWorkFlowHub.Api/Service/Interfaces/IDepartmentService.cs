using EmployeeWorkFlowHub.Models;

namespace EmployeeWorkFlowHub.Service.Interfaces
{
    /// <summary>
    /// Service interface for Department operations.
    /// </summary>
    public interface IDepartmentService
    {
        Task<ResultArgs> GetAllAsync();
        Task<ResultArgs> GetByIdAsync(int id);
        Task<ResultArgs> CreateAsync(Department department);
        Task<ResultArgs> UpdateAsync(Department department);
        Task<ResultArgs> DeleteAsync(int id);
    }
}
