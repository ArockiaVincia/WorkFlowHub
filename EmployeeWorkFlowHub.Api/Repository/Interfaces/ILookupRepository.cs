using EmployeeWorkFlowHub.Models;

namespace EmployeeWorkFlowHub.Repository.Interfaces
{
    /// <summary>
    /// Repository interface for support data / lookup master operations.
    /// </summary>
    public interface ILookupRepository
    {
        Task<List<SupportDataItem>> GetAllAsync();
        Task<List<SupportDataItem>> GetByTypeAsync(string type);
    }
}
