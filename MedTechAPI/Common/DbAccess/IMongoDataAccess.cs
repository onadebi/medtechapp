using MongoDB.Driver;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace Common.DbAccess
{
    public interface IMongoDataAccess
    {
        IMongoCollection<T> GetCollection<T>();
        Task<string> CreateUniqueIndex<T>(IMongoCollection<T> collection, string fieldName, [CallerMemberName] string callerName = "");
        Task<List<T>> GetData<T>(Expression<Func<T, bool>> predicate = null, [CallerMemberName] string callerName = "");
        Task<T> InsertRecord<T>(T entry, [CallerMemberName] string callerName = "");
    }
}
