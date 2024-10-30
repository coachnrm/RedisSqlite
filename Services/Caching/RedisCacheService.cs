using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using StackExchange.Redis;

namespace RedisSqlite.Services.Caching
{
    public class RedisCacheService : IRedisCacheService
    {
        //private readonly IDistributedCache? _cache;
        private readonly IDatabase _database;
        public RedisCacheService(IConnectionMultiplexer connectionMultiplexer)
        {
            _database = connectionMultiplexer.GetDatabase();
        }
        public T? GetData<T>(string key)
        {
            // var data = _cache?.GetString(key);

            // if (data is null)
            //     return default(T);

            // return JsonSerializer.Deserialize<T>(data)!;
            var data = _database.StringGet(key);
            if (data.IsNull)
                return default;

            return JsonSerializer.Deserialize<T>(data);
        }

        public void SetData<T>(string key, T data, TimeSpan? expiration = null)
        {
            // var options = new DistributedCacheEntryOptions()
            // {
            //     AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
            // };

            // _cache?.SetString(key, JsonSerializer.Serialize(data), options);
            var jsonData = JsonSerializer.Serialize(data);
            _database.StringSet(key, jsonData, expiration);
        }

        public void RemoveData(string key)
        {
            _database.KeyDelete(key);
        }
    }
}