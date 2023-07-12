using MongoDB.Driver;
using MongoDB.Driver.Linq;
using System.Data;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace Common.DbAccess
{
    public class MongoDataAccess : IMongoDataAccess
    {
        //private readonly ILogger<MongoDataAccess> _logger;
        private readonly IMongoDatabase _db;
        public MongoDataAccess(string conString, string database)
        {
            _db = new MongoClient(conString).GetDatabase(database);
        }

        public IMongoCollection<T> GetCollection<T>()
        {
            var col =  _db.GetCollection<T>(typeof(T).Name);
            return col;
        }

        public async Task<string> CreateUniqueIndex<T>(IMongoCollection<T> collection, string indexName, [CallerMemberName] string callerName = "")
        {
            string indexNameResult = string.Empty;
            try
            {
                List<AppBsonIndexes> indexes = new List<AppBsonIndexes>();
                var allIndexes = await (await collection.Indexes.ListAsync()).ToListAsync();
                for(int i =0; i<allIndexes.Count; i++)
                {
                    var index = allIndexes[i];
                    var itemDictionary = index.ToDictionary();
                    string indexString = System.Text.Json.JsonSerializer.Serialize(itemDictionary);
                    var desItem = System.Text.Json.JsonSerializer.Deserialize<AppBsonIndexes>(indexString, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
                    indexes.Add(desItem);
                };

                // If index not present create it else skip.
                if (indexes.Where(i => i.Name.Equals(indexName,StringComparison.CurrentCultureIgnoreCase)).Any() == false)
                {
                    // Create Index here
                    indexName = collection.Indexes.CreateOne(
                    new CreateIndexModel<T>(Builders<T>.IndexKeys.Ascending(m => indexName),
                    new CreateIndexOptions { Unique = true })
                    );
                }
                else
                {
                    indexNameResult = $"Index name ${indexName} already exists";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{callerName}] ::> {ex.Message}");
            }
            return indexNameResult;
        }

        public async Task<List<T>> GetData<T>(Expression<Func<T, bool>> predicate = null, [CallerMemberName] string callerName = "")
        {
            List<T> objResp = new();
            try
            {
                if (predicate != null)
                {
                    objResp = await _db.GetCollection<T>(typeof(T).Name).Find(predicate).ToListAsync();

                }
                else
                {
                    objResp = await _db.GetCollection<T>(typeof(T).Name).Find(_ => true).ToListAsync();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{callerName}] ::> {ex.Message}");
            }
            return objResp;
        }

        public async Task<T> InsertRecord<T>(T entry, [CallerMemberName] string callerName = "")
        {
            try
            {
                await _db.GetCollection<T>(typeof(T).Name).InsertOneAsync(entry);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[{callerName}] ::> {ex.Message}");
            }
            return entry;
        }


    }

    public class AppBsonIndexes
    {
        public int V { get; set; }
        public bool Unique { get; set; }
        //public int key { get; set; }
        public string Name { get; set; }
        public string Ns { get; set; }
    }

}
