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
    /// Implements ILookupRepository using Dapper and sp_Lookup_CRUD.
    /// Maps records to standard SupportDataItem with id, code, value, type.
    /// </summary>
    public class LookupRepository : ILookupRepository
    {
        private readonly string _connectionString;

        public LookupRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString(CommonVariable.ConnectionString.DefaultConnection)
                ?? configuration.GetConnectionString("DefaultConnection")!;
        }

        private IDbConnection CreateConnection() => new SqlConnection(_connectionString);

        public async Task<List<SupportDataItem>> GetAllAsync()
        {
            var list = new List<SupportDataItem>();
            var parameters = new DynamicParameters();
            try
            {
                // 1: SELECT_ALL
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Lookup.SelectAll, DbType.Int32, ParameterDirection.Input);

                using var connection = CreateConnection();
                var result = await connection.QueryAsync<dynamic>(
                    StroredProc.Lookup.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                if (result != null)
                {
                    list = result.Select(r => new SupportDataItem
                    {
                        Id = (int)r.Id,
                        Code = (string)(r.LookupCode ?? string.Empty),
                        Value = (string)(r.LookupValue ?? string.Empty),
                        Type = (string)(r.LookupType ?? string.Empty)
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                new ErrorLog().WriteLog(ex);
            }
            return list;
        }

        public async Task<List<SupportDataItem>> GetByTypeAsync(string type)
        {
            var list = new List<SupportDataItem>();
            var parameters = new DynamicParameters();
            try
            {
                // 2: SELECT_BY_TYPE
                parameters.Add(CommonVariable.Parameter.ActionId, CommonVariable.ActionId.Lookup.SelectByType, DbType.Int32, ParameterDirection.Input);
                parameters.Add(CommonVariable.Parameter.LookupType, type, DbType.String, ParameterDirection.Input);

                using var connection = CreateConnection();
                var result = await connection.QueryAsync<dynamic>(
                    StroredProc.Lookup.CRUD,
                    parameters,
                    commandType: CommandType.StoredProcedure);

                if (result != null)
                {
                    list = result.Select(r => new SupportDataItem
                    {
                        Id = (int)r.Id,
                        Code = (string)(r.LookupCode ?? string.Empty),
                        Value = (string)(r.LookupValue ?? string.Empty),
                        Type = (string)(r.LookupType ?? string.Empty)
                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                new ErrorLog().WriteLog(ex);
            }
            return list;
        }
    }
}
