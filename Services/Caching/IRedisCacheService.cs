namespace RedisSqlite.Services.Caching
{
    public interface IRedisCacheService
    {
        // T? GetData<T>(string key);
        // void SetData<T>(string key, T data);
        T? GetData<T>(string key);
        void SetData<T>(string key, T data, TimeSpan? expiration = null);
        void RemoveData(string key);
    }
}