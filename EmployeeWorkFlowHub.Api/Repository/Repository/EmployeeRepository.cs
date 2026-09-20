using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using EmployeeWorkFlowHub.Common;
using EmployeeWorkFlowHub.Common.Helper;
using EmployeeWorkFlowHub.Models;
using EmployeeWorkFlowHub.Repository.Interfaces;

namespace EmployeeWorkFlowHub.Repository.Repository
{
    /// <summary>
    /// Implements IEmployeeRepository using Dapper and sp_Employee_CRUD.
    /// </summary>
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly string _connectionString;

        public EmployeeRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString(CommonVariable.ConnectionString.DefaultConnection)
                ?? configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<List<Employee>> GetAllAsync()
        {
            var list = new List<Employee>();
            var parameters = new DynamicParameters();
            try
            {
                // 1: SELECT_ALL
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Employee.SelectAll, DbType.Int32, ParameterDirection.Input);

                using var connection = CreateConnection();
                var result = await connection.QueryAsync<Employee>(
                    StroredProc.Employee.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                if (result != null)
                {
                    list = result.ToList();
                }
            }
            catch (Exception ex)
            {
                new ErrorLog().WriteLog(ex);
            }
            return list;
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            var parameters = new DynamicParameters();
            try
            {
                // 2: SELECT_BY_ID
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Employee.SelectById, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Id, id, DbType.Int32, ParameterDirection.Input);

                using var connection = CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<Employee>(
                    StroredProc.Employee.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                new ErrorLog().WriteLog(ex);
                return null;
            }
        }

        public async Task<ResultArgs> InsertAsync(Employee employee)
        {
            var result = new ResultArgs();
            var parameters = new DynamicParameters();
            try
            {
                string role = !string.IsNullOrWhiteSpace(employee.Role) ? employee.Role : CommonVariable.RoleName.Developer;
                string username = !string.IsNullOrWhiteSpace(employee.Username)
                    ? employee.Username.Trim()
                    : (!string.IsNullOrWhiteSpace(employee.FullName) ? employee.FullName.Trim() : employee.EmployeeCode.Trim());
                string rawPassword = !string.IsNullOrWhiteSpace(employee.Password) ? employee.Password : "Password@123";
                string passwordHash = PasswordHasher.HashPassword(rawPassword);

                // 3: INSERT
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Employee.Insert, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.EmployeeCode, employee.EmployeeCode.Trim(), DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.FullName, employee.FullName.Trim(), DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Email, employee.Email.Trim(), DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.DepartmentId, employee.DepartmentId, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Designation, employee.Designation?.Trim() ?? string.Empty, DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.IsActive, employee.IsActive, DbType.Boolean, ParameterDirection.Input);
                parameters.Add("@Role", role, DbType.String, ParameterDirection.Input);
                parameters.Add("@Username", username, DbType.String, ParameterDirection.Input);
                parameters.Add("@PasswordHash", passwordHash, DbType.String, ParameterDirection.Input);

                using var connection = CreateConnection();
                var newId = await connection.ExecuteScalarAsync<int>(
                    StroredProc.Employee.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                result.StatusCode = 200;
                result.StatusMessage = "Employee created successfully.";
                result.ResultData = await GetByIdAsync(newId) ?? employee;
            }
            catch (Exception ex)
            {
                new ErrorLog().WriteLog(ex);
                result.StatusCode = 500;
                result.StatusMessage = ex.Message;
            }
            return result;
        }

        public async Task<ResultArgs> UpdateAsync(Employee employee)
        {
            var result = new ResultArgs();
            var parameters = new DynamicParameters();
            try
            {
                string role = !string.IsNullOrWhiteSpace(employee.Role) ? employee.Role : CommonVariable.RoleName.Developer;
                string? passwordHash = !string.IsNullOrWhiteSpace(employee.Password) ? PasswordHasher.HashPassword(employee.Password) : null;
                string username = !string.IsNullOrWhiteSpace(employee.Username)
                    ? employee.Username.Trim()
                    : (!string.IsNullOrWhiteSpace(employee.FullName) ? employee.FullName.Trim() : employee.EmployeeCode.Trim());

                // 4: UPDATE
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Employee.Update, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Id, employee.Id, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.EmployeeCode, employee.EmployeeCode.Trim(), DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.FullName, employee.FullName.Trim(), DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Email, employee.Email.Trim(), DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.DepartmentId, employee.DepartmentId, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Designation, employee.Designation?.Trim() ?? string.Empty, DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.IsActive, employee.IsActive, DbType.Boolean, ParameterDirection.Input);
                parameters.Add("@Role", role, DbType.String, ParameterDirection.Input);
                parameters.Add("@Username", username, DbType.String, ParameterDirection.Input);
                parameters.Add("@PasswordHash", passwordHash, DbType.String, ParameterDirection.Input);

                using var connection = CreateConnection();
                await connection.ExecuteAsync(
                    StroredProc.Employee.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                result.StatusCode = 200;
                result.StatusMessage = "Employee updated successfully.";
                result.ResultData = await GetByIdAsync(employee.Id) ?? employee;
            }
            catch (Exception ex)
            {
                new ErrorLog().WriteLog(ex);
                result.StatusCode = 500;
                result.StatusMessage = ex.Message;
            }
            return result;
        }

        public async Task<ResultArgs> DeleteAsync(int id)
        {
            var result = new ResultArgs();
            var parameters = new DynamicParameters();
            try
            {
                using var connection = CreateConnection();

                // 5: DELETE
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Employee.Delete, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Id, id, DbType.Int32, ParameterDirection.Input);

                await connection.ExecuteAsync(
                    StroredProc.Employee.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                result.StatusCode = 200;
                result.StatusMessage = "Employee deleted successfully.";
            }
            catch (Exception ex)
            {
                new ErrorLog().WriteLog(ex);
                result.StatusCode = 500;
                result.StatusMessage = ex.Message;
            }
            return result;
        }
    }
}
