using EmployeeWorkFlowHub.Models;

namespace EmployeeWorkFlowHub.Repository.Interfaces
{
    /// <summary>
    /// Repository interface for Department data operations.
    /// </summary>
    public interface IDepartmentRepository
    {
        Task<List<Department>> GetAllAsync();
        Task<Department?> GetByIdAsync(int id);
        Task<ResultArgs> InsertAsync(Department department);
        Task<ResultArgs> UpdateAsync(Department department);
        Task<ResultArgs> DeleteAsync(int id);
    }
}
