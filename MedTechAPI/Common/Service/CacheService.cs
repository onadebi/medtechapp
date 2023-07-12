using System.Text.Json;
using Common.Interface;
using StackExchange.Redis;

namespace Common.Service
{
    public class CacheService : ICacheService
    {
        private readonly IDatabase _db;
        private readonly string _appKey;
        public CacheService(IConnectionMultiplexer connectionMultiplxer, string appKey)
        {
            _db = connectionMultiplxer.GetDatabase();
            _appKey = appKey;
        }
        public async Task<T> GetData<T>(string key)
        {
            var result = await _db.StringGetAsync($"{_appKey}:{key}");
            return result.HasValue ? JsonSerializer.Deserialize<T>(result) : default;
        }

        public async Task<T> GetSessionData<T>(string key)
        {
            var result = await _db.StringGetAsync($"{_appKey}Session:{key}");
            return result.HasValue ? JsonSerializer.Deserialize<T>(result) : default;
        }
        public async Task<bool> SetSessionData<T>(string key, T value, int ttl)
        {
            TimeSpan expiryTime = TimeSpan.FromSeconds(ttl);
            bool isSet = false;
            try
            {
                isSet = await _db.StringSetAsync($"{_appKey}Session:{key}", JsonSerializer.Serialize(value), expiryTime);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return isSet;
        }

        public async Task<bool> RemoveData(string key)
        {
            bool _isKeyExist = _db.KeyExists($"{_appKey}:{key}");
            if (_isKeyExist == true)
            {
                return await _db.KeyDeleteAsync($"{_appKey}:{key}");
            }
            return false;
        }

        public async Task<bool> SetData<T>(string key, T value, int ttl)
        {
            TimeSpan expiryTime = TimeSpan.FromSeconds(ttl); // ttl.DateTime.Subtract(DateTime.Now);
            bool isSet = false;
            try
            {
                isSet = await _db.StringSetAsync($"{_appKey}:{key}", JsonSerializer.Serialize(value), expiryTime);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return isSet;
        }
    }
}
