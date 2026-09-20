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
    /// Implements ITaskRepository using Dapper and sp_Task_CRUD.
    /// </summary>
    public class TaskRepository : ITaskRepository
    {
        private readonly string _connectionString;

        public TaskRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString(CommonVariable.ConnectionString.DefaultConnection)
                ?? configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<List<TaskItem>> GetAllAsync()
        {
            var list = new List<TaskItem>();
            var parameters = new DynamicParameters();
            try
            {
                // 1: SELECT_ALL
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Task.SelectAll, DbType.Int32, ParameterDirection.Input);

                using var connection = CreateConnection();
                var result = await connection.QueryAsync<TaskItem>(
                    StroredProc.Task.CRUD,
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

        public async Task<TaskItem?> GetByIdAsync(int id)
        {
            var parameters = new DynamicParameters();
            try
            {
                // 2: SELECT_BY_ID
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Task.SelectById, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Id, id, DbType.Int32, ParameterDirection.Input);

                using var connection = CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<TaskItem>(
                    StroredProc.Task.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                new ErrorLog().WriteLog(ex);
                return null;
            }
        }

        public async Task<List<TaskItem>> GetByProjectLeadAsync(int leadEmployeeId)
        {
            var list = new List<TaskItem>();
            var parameters = new DynamicParameters();
            try
            {
                // 3: SELECT_BY_PROJECT_LEAD
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Task.SelectByProjectLead, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.EmployeeId, leadEmployeeId, DbType.Int32, ParameterDirection.Input);

                using var connection = CreateConnection();
                var result = await connection.QueryAsync<TaskItem>(
                    StroredProc.Task.CRUD,
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

        public async Task<List<TaskItem>> GetByEmployeeAsync(int employeeId)
        {
            var list = new List<TaskItem>();
            var parameters = new DynamicParameters();
            try
            {
                // 4: SELECT_BY_EMPLOYEE
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Task.SelectByEmployee, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.EmployeeId, employeeId, DbType.Int32, ParameterDirection.Input);

                using var connection = CreateConnection();
                var result = await connection.QueryAsync<TaskItem>(
                    StroredProc.Task.CRUD,
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

        public async Task<List<TaskItem>> GetForQCAsync()
        {
            var list = new List<TaskItem>();
            var parameters = new DynamicParameters();
            try
            {
                // 5: SELECT_FOR_QC
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Task.SelectForQC, DbType.Int32, ParameterDirection.Input);

                using var connection = CreateConnection();
                var result = await connection.QueryAsync<TaskItem>(
                    StroredProc.Task.CRUD,
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

        public async Task<ResultArgs> InsertAsync(TaskItem task)
        {
            var result = new ResultArgs();
            var parameters = new DynamicParameters();
            try
            {
                // 6: INSERT
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Task.Insert, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Title, task.Title.Trim(), DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Description, task.Description?.Trim(), DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.ProjectId, task.ProjectId, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.EmployeeId, task.EmployeeId, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Priority, string.IsNullOrWhiteSpace(task.Priority) ? "Medium" : task.Priority.Trim(), DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.DueDate, task.DueDate, DbType.DateTime, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Status, string.IsNullOrWhiteSpace(task.Status) ? "To Do" : task.Status.Trim(), DbType.String, ParameterDirection.Input);

                using var connection = CreateConnection();
                var newId = await connection.ExecuteScalarAsync<int>(
                    StroredProc.Task.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                result.StatusCode = 200;
                result.StatusMessage = "Task created successfully.";
                result.ResultData = await GetByIdAsync(newId) ?? task;
            }
            catch (Exception ex)
            {
                new ErrorLog().WriteLog(ex);
                result.StatusCode = 500;
                result.StatusMessage = ex.Message;
            }
            return result;
        }

        public async Task<ResultArgs> UpdateAsync(TaskItem task)
        {
            var result = new ResultArgs();
            var parameters = new DynamicParameters();
            try
            {
                // 7: UPDATE
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Task.Update, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Id, task.Id, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Title, task.Title.Trim(), DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Description, task.Description?.Trim(), DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.ProjectId, task.ProjectId, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.EmployeeId, task.EmployeeId, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Priority, string.IsNullOrWhiteSpace(task.Priority) ? "Medium" : task.Priority.Trim(), DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.DueDate, task.DueDate, DbType.DateTime, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Status, string.IsNullOrWhiteSpace(task.Status) ? "To Do" : task.Status.Trim(), DbType.String, ParameterDirection.Input);

                using var connection = CreateConnection();
                await connection.ExecuteAsync(
                    StroredProc.Task.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                result.StatusCode = 200;
                result.StatusMessage = "Task updated successfully.";
                result.ResultData = await GetByIdAsync(task.Id) ?? task;
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
                // 8: DELETE
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Task.Delete, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Id, id, DbType.Int32, ParameterDirection.Input);

                using var connection = CreateConnection();
                await connection.ExecuteAsync(
                    StroredProc.Task.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                result.StatusCode = 200;
                result.StatusMessage = "Task deleted successfully.";
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
