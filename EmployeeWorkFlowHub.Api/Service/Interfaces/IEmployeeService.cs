using EmployeeWorkFlowHub.Models;

namespace EmployeeWorkFlowHub.Service.Interfaces
{
    /// <summary>
    /// Service interface for Employee operations.
    /// </summary>
    public interface IEmployeeService
    {
        Task<ResultArgs> GetAllAsync();
        Task<ResultArgs> GetByIdAsync(int id);
        Task<ResultArgs> CreateAsync(Employee employee);
        Task<ResultArgs> UpdateAsync(Employee employee);
        Task<ResultArgs> DeleteAsync(int id);
    }
}
