using EmployeeWorkFlowHub.Models;
using EmployeeWorkFlowHub.Repository.Interfaces;
using EmployeeWorkFlowHub.Service.Interfaces;

namespace EmployeeWorkFlowHub.Service.Services
{
    /// <summary>
    /// Implements IDepartmentService for business logic and data orchestration.
    /// Injects IDepartmentRepository and returns ResultArgs.
    /// </summary>
    public class DepartmentService : IDepartmentService
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentService(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        public async Task<ResultArgs> GetAllAsync()
        {
            var result = new ResultArgs();
            var list = await _departmentRepository.GetAllAsync();
            result.StatusCode = 200;
            result.StatusMessage = "Departments retrieved successfully.";
            result.ResultData = list;
            return result;
        }

        public async Task<ResultArgs> GetByIdAsync(int id)
        {
            var result = new ResultArgs();
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
            {
                result.StatusCode = 404;
                result.StatusMessage = $"Department with ID {id} was not found.";
                return result;
            }

            result.StatusCode = 200;
            result.StatusMessage = "Department retrieved successfully.";
            result.ResultData = department;
            return result;
        }

        public async Task<ResultArgs> CreateAsync(Department department)
        {
            if (string.IsNullOrWhiteSpace(department.Name))
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Department name is required." };
            }

            return await _departmentRepository.InsertAsync(department);
        }

        public async Task<ResultArgs> UpdateAsync(Department department)
        {
            if (department.Id <= 0)
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Valid department ID is required." };
            }
            if (string.IsNullOrWhiteSpace(department.Name))
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Department name is required." };
            }

            var existing = await _departmentRepository.GetByIdAsync(department.Id);
            if (existing == null)
            {
                return new ResultArgs { StatusCode = 404, StatusMessage = $"Department with ID {department.Id} was not found." };
            }

            return await _departmentRepository.UpdateAsync(department);
        }

        public async Task<ResultArgs> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Valid department ID is required." };
            }

            var existing = await _departmentRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return new ResultArgs { StatusCode = 404, StatusMessage = $"Department with ID {id} was not found." };
            }

            return await _departmentRepository.DeleteAsync(id);
        }
    }
}
