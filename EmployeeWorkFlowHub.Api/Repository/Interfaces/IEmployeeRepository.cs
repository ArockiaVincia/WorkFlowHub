using EmployeeWorkFlowHub.Models;

namespace EmployeeWorkFlowHub.Repository.Interfaces
{
    /// <summary>
    /// Repository interface for Employee data operations.
    /// </summary>
    public interface IEmployeeRepository
    {
        Task<List<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task<ResultArgs> InsertAsync(Employee employee);
        Task<ResultArgs> UpdateAsync(Employee employee);
        Task<ResultArgs> DeleteAsync(int id);
    }
}
