using EmployeeWorkFlowHub.Models;
using EmployeeWorkFlowHub.Repository.Interfaces;
using EmployeeWorkFlowHub.Service.Interfaces;

namespace EmployeeWorkFlowHub.Service.Services
{
    /// <summary>
    /// Implements ILookupService for business logic and data orchestration.
    /// Injects ILookupRepository and returns ResultArgs.
    /// </summary>
    public class LookupService : ILookupService
    {
        private readonly ILookupRepository _lookupRepository;

        public LookupService(ILookupRepository lookupRepository)
        {
            _lookupRepository = lookupRepository;
        }

        public async Task<ResultArgs> GetAllAsync()
        {
            var result = new ResultArgs();
            var items = await _lookupRepository.GetAllAsync();
            result.StatusCode = 200;
            result.StatusMessage = "Lookup items retrieved successfully.";
            result.ResultData = items;
            return result;
        }

        public async Task<ResultArgs> GetByTypeAsync(string type)
        {
            var result = new ResultArgs();
            if (string.IsNullOrWhiteSpace(type))
            {
                result.StatusCode = 400;
                result.StatusMessage = "Lookup category type is required.";
                return result;
            }

            var items = await _lookupRepository.GetByTypeAsync(type.Trim());
            result.StatusCode = 200;
            result.StatusMessage = "Category lookup items retrieved successfully.";
            result.ResultData = items;
            return result;
        }
    }
}
