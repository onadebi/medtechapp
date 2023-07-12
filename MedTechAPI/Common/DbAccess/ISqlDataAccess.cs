using System.Data;
using System.Runtime.CompilerServices;

namespace Common.DbAccess
{
    public interface ISqlDataAccess
    {
        Task<IEnumerable<T>> GetData<T, U>(string queryString, U parameters, CommandType commandType = CommandType.Text,[CallerMemberName] string callerName = "");
        Task<int> SaveData<T>(string queryString, T parameters, CommandType commandType = CommandType.Text, [CallerMemberName] string callerName = "");
    }
}
