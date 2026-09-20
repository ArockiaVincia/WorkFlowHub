using EmployeeWorkFlowHub.Models;

namespace EmployeeWorkFlowHub.Service.Interfaces
{
    /// <summary>
    /// Service interface for Lookup master operations for data access operations.
    /// </summary>
    public interface ILookupService
    {
        Task<ResultArgs> GetAllAsync();
        Task<ResultArgs> GetByTypeAsync(string type);
    }
}
