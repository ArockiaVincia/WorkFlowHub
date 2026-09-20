using EmployeeWorkFlowHub.Models;
using EmployeeWorkFlowHub.Repository.Interfaces;
using EmployeeWorkFlowHub.Service.Interfaces;

namespace EmployeeWorkFlowHub.Service.Services
{
    /// <summary>
    /// Implements IEmployeeService for business logic and data orchestration.
    /// Injects IEmployeeRepository and returns ResultArgs.
    /// </summary>
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;

        public EmployeeService(IEmployeeRepository employeeRepository)
        {
            _employeeRepository = employeeRepository;
        }

        public async Task<ResultArgs> GetAllAsync()
        {
            var result = new ResultArgs();
            var list = await _employeeRepository.GetAllAsync();
            result.StatusCode = 200;
            result.StatusMessage = "Employees retrieved successfully.";
            result.ResultData = list;
            return result;
        }

        public async Task<ResultArgs> GetByIdAsync(int id)
        {
            var result = new ResultArgs();
            var employee = await _employeeRepository.GetByIdAsync(id);
            if (employee == null)
            {
                result.StatusCode = 404;
                result.StatusMessage = $"Employee with ID {id} was not found.";
                return result;
            }

            result.StatusCode = 200;
            result.StatusMessage = "Employee retrieved successfully.";
            result.ResultData = employee;
            return result;
        }

        public async Task<ResultArgs> CreateAsync(Employee employee)
        {
            if (string.IsNullOrWhiteSpace(employee.FullName))
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Full name is required." };
            }
            if (string.IsNullOrWhiteSpace(employee.EmployeeCode))
            {
                employee.EmployeeCode = await GenerateEmployeeCodeAsync();
            }
            if (string.IsNullOrWhiteSpace(employee.Email))
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Email address is required." };
            }
            if (employee.DepartmentId <= 0)
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Valid department is required." };
            }

            return await _employeeRepository.InsertAsync(employee);
        }

        public async Task<ResultArgs> UpdateAsync(Employee employee)
        {
            if (employee.Id <= 0)
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Valid employee ID is required." };
            }
            if (string.IsNullOrWhiteSpace(employee.FullName))
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Full name is required." };
            }

            var existing = await _employeeRepository.GetByIdAsync(employee.Id);
            if (existing == null)
            {
                return new ResultArgs { StatusCode = 404, StatusMessage = $"Employee with ID {employee.Id} was not found." };
            }

            return await _employeeRepository.UpdateAsync(employee);
        }

        public async Task<ResultArgs> DeleteAsync(int id)
        {
            if (id <= 0)
            {
                return new ResultArgs { StatusCode = 400, StatusMessage = "Valid employee ID is required." };
            }

            var existing = await _employeeRepository.GetByIdAsync(id);
            if (existing == null)
            {
                return new ResultArgs { StatusCode = 404, StatusMessage = $"Employee with ID {id} was not found." };
            }

            return await _employeeRepository.DeleteAsync(id);
        }

        private async Task<string> GenerateEmployeeCodeAsync()
        {
            var list = await _employeeRepository.GetAllAsync();
            var existingCodes = new HashSet<string>(
                list.Select(e => e.EmployeeCode?.Trim().ToUpper() ?? string.Empty),
                StringComparer.OrdinalIgnoreCase);

            int num = 1001;
            while (existingCodes.Contains($"EMP-{num}"))
            {
                num++;
            }

            return $"EMP-{num}";
        }
    }
}
