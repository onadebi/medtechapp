using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Common.DbAccess
{
    public class SqlDataAccess : ISqlDataAccess
    {
        private readonly string _conString;
        private readonly ILogger<SqlDataAccess> _logger;

        public SqlDataAccess(ILogger<SqlDataAccess> logger, string conString)
        {
            this._conString = conString;
            _logger = logger;
        }

        public async Task<IEnumerable<T>> GetData<T, U>(string queryString, U parameters, CommandType commandType = CommandType.Text, [CallerMemberName] string callerName = "")
        {
            using IDbConnection conn = new NpgsqlConnection(_conString);
            IEnumerable<T> objResp = Array.Empty<T>();
            try
            {
                objResp = await conn.QueryAsync<T>(queryString, param: parameters, commandType: commandType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, callerName);
            }
            return objResp;
        }

        public async Task<int> SaveData<T>(string queryString, T parameters, CommandType commandType = CommandType.Text, [CallerMemberName] string callerName = "")
        {
            int countOfRecordsModified = 0;
            _logger.LogInformation($"{callerName} for userdata:: {JsonSerializer.Serialize(parameters)}");
            try
            {
                using IDbConnection conn = new NpgsqlConnection(_conString);
                conn.Open();
                countOfRecordsModified = await conn.ExecuteAsync(queryString, param: parameters, commandType: commandType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, callerName);
            }
            return countOfRecordsModified;
        }
    }
}
