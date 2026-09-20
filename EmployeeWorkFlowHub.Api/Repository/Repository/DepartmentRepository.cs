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
    /// Implements IDepartmentRepository using Dapper and sp_Department_CRUD.
    /// </summary>
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly string _connectionString;

        public DepartmentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString(CommonVariable.ConnectionString.DefaultConnection)
                ?? configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<List<Department>> GetAllAsync()
        {
            var list = new List<Department>();
            var parameters = new DynamicParameters();
            try
            {
                // 1: SELECT_ALL
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Department.SelectAll, DbType.Int32, ParameterDirection.Input);

                using var connection = CreateConnection();
                var result = await connection.QueryAsync<Department>(
                    StroredProc.Department.CRUD,
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

        public async Task<Department?> GetByIdAsync(int id)
        {
            var parameters = new DynamicParameters();
            try
            {
                // 2: SELECT_BY_ID
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Department.SelectById, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Id, id, DbType.Int32, ParameterDirection.Input);

                using var connection = CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<Department>(
                    StroredProc.Department.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                new ErrorLog().WriteLog(ex);
                return null;
            }
        }

        public async Task<ResultArgs> InsertAsync(Department department)
        {
            var result = new ResultArgs();
            var parameters = new DynamicParameters();
            try
            {
                // 3: INSERT
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Department.Insert, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Name, department.Name.Trim(), DbType.String, ParameterDirection.Input);

                using var connection = CreateConnection();
                var newId = await connection.ExecuteScalarAsync<int>(
                    StroredProc.Department.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                result.StatusCode = 200;
                result.StatusMessage = "Department created successfully.";
                department.Id = newId;
                result.ResultData = department;
            }
            catch (Exception ex)
            {
                new ErrorLog().WriteLog(ex);
                result.StatusCode = 500;
                result.StatusMessage = ex.Message;
            }
            return result;
        }

        public async Task<ResultArgs> UpdateAsync(Department department)
        {
            var result = new ResultArgs();
            var parameters = new DynamicParameters();
            try
            {
                // 4: UPDATE
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Department.Update, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Id, department.Id, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Name, department.Name.Trim(), DbType.String, ParameterDirection.Input);

                using var connection = CreateConnection();
                await connection.ExecuteAsync(
                    StroredProc.Department.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                result.StatusCode = 200;
                result.StatusMessage = "Department updated successfully.";
                result.ResultData = department;
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
                // 5: DELETE
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Department.Delete, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Id, id, DbType.Int32, ParameterDirection.Input);

                using var connection = CreateConnection();
                await connection.ExecuteAsync(
                    StroredProc.Department.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                result.StatusCode = 200;
                result.StatusMessage = "Department deleted successfully.";
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
