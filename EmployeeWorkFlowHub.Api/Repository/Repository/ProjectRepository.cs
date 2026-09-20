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
    /// Implements IProjectRepository using Dapper and sp_Project_CRUD.
    /// </summary>
    public class ProjectRepository : IProjectRepository
    {
        private readonly string _connectionString;

        public ProjectRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString(CommonVariable.ConnectionString.DefaultConnection)
                ?? configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<List<Project>> GetAllAsync()
        {
            var list = new List<Project>();
            var parameters = new DynamicParameters();
            try
            {
                // 1: SELECT_ALL
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Project.SelectAll, DbType.Int32, ParameterDirection.Input);

                using var connection = CreateConnection();
                var result = await connection.QueryAsync<Project>(
                    StroredProc.Project.CRUD,
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

        public async Task<Project?> GetByIdAsync(int id)
        {
            var parameters = new DynamicParameters();
            try
            {
                // 2: SELECT_BY_ID
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Project.SelectById, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Id, id, DbType.Int32, ParameterDirection.Input);

                using var connection = CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<Project>(
                    StroredProc.Project.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                new ErrorLog().WriteLog(ex);
                return null;
            }
        }

        public async Task<List<Project>> GetByManagerIdAsync(int managerId)
        {
            var list = new List<Project>();
            var parameters = new DynamicParameters();
            try
            {
                // 3: SELECT_BY_MANAGER
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Project.SelectByManager, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.ProjectManagerId, managerId, DbType.Int32, ParameterDirection.Input);

                using var connection = CreateConnection();
                var result = await connection.QueryAsync<Project>(
                    StroredProc.Project.CRUD,
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

        public async Task<ResultArgs> InsertAsync(Project project)
        {
            var result = new ResultArgs();
            var parameters = new DynamicParameters();
            try
            {
                // 4: INSERT
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Project.Insert, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Name, project.Name.Trim(), DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.DepartmentId, project.DepartmentId, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.ProjectManagerId, project.ProjectManagerId, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.TeamMembers, project.TeamMembers, DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Status, string.IsNullOrWhiteSpace(project.Status) ? "In Progress" : project.Status.Trim(), DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.StartDate, project.StartDate, DbType.DateTime, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.EndDate, project.EndDate, DbType.DateTime, ParameterDirection.Input);

                using var connection = CreateConnection();
                var newId = await connection.ExecuteScalarAsync<int>(
                    StroredProc.Project.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                result.StatusCode = 200;
                result.StatusMessage = "Project created successfully.";
                result.ResultData = await GetByIdAsync(newId) ?? project;
            }
            catch (Exception ex)
            {
                new ErrorLog().WriteLog(ex);
                result.StatusCode = 500;
                result.StatusMessage = ex.Message;
            }
            return result;
        }

        public async Task<ResultArgs> UpdateAsync(Project project)
        {
            var result = new ResultArgs();
            var parameters = new DynamicParameters();
            try
            {
                // 5: UPDATE
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Project.Update, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Id, project.Id, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Name, project.Name.Trim(), DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.DepartmentId, project.DepartmentId, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.ProjectManagerId, project.ProjectManagerId, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.TeamMembers, project.TeamMembers, DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Status, string.IsNullOrWhiteSpace(project.Status) ? "In Progress" : project.Status.Trim(), DbType.String, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.StartDate, project.StartDate, DbType.DateTime, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.EndDate, project.EndDate, DbType.DateTime, ParameterDirection.Input);

                using var connection = CreateConnection();
                await connection.ExecuteAsync(
                    StroredProc.Project.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                result.StatusCode = 200;
                result.StatusMessage = "Project updated successfully.";
                result.ResultData = await GetByIdAsync(project.Id) ?? project;
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
                // 6: DELETE
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Project.Delete, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.Id, id, DbType.Int32, ParameterDirection.Input);

                using var connection = CreateConnection();
                await connection.ExecuteAsync(
                    StroredProc.Project.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                result.StatusCode = 200;
                result.StatusMessage = "Project deleted successfully.";
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
