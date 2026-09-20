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
    /// Implements IAuthenticateRepository using Dapper and sp_User_Authenticate.
    /// </summary>
    public class AuthenticateRepository : IAuthenticateRepository
    {
        private readonly string _connectionString;

        public AuthenticateRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString(CommonVariable.ConnectionString.DefaultConnection)
                ?? configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<User?> GetByUsernameAsync(string username)
        {
            var parameters = new DynamicParameters();
            try
            {
                parameters.Add(CommonVariable.Parameter.Username, username.Trim(), DbType.String, ParameterDirection.Input);

                using var connection = CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<User>(
                    StroredProc.User.Authenticate,
                    parameters,
                    commandType: CommandType.StoredProcedure);
            }
            catch (Exception ex)
            {
                new ErrorLog().WriteLog(ex);
                return null;
            }
        }

        public async Task<User?> GetByIdAsync(int id)
        {
            try
            {
                using var connection = CreateConnection();
                return await connection.QueryFirstOrDefaultAsync<User>(@"
                    SELECT 
                        [Id],
                        COALESCE([Username], [EmployeeCode]) AS [Username],
                        [PasswordHash],
                        [Role],
                        [FullName],
                        [IsActive],
                        [Id] AS [EmployeeId]
                    FROM [dbo].[Employees]
                    WHERE [Id] = @Id;", new { Id = id });
            }
            catch (Exception ex)
            {
                new ErrorLog().WriteLog(ex);
                return null;
            }
        }
    }
}
