using System.Text;
using System.Text.Json;
using Jobs.Common.Contracts;
using Jobs.Common.DataModel;
using Jobs.Common.Extentions;
using Microsoft.Extensions.Caching.Distributed;
using DistributedCacheExtensions = Jobs.Common.Extentions.DistributedCacheExtensions;

namespace Jobs.Common.Providers;

public class RedisOrDragonflyDbApiKeyStorageServiceProvider(IDistributedCache cache) : IApiKeyStorageServiceProvider
{
    public bool IsKeyValid(string key)
    {
        var result = cache.TryGetValue(key, out ApiKey apiKey);
        
        if (apiKey == null)
            return false;
        
        if (result && !apiKey.Expiration.HasValue)
        {
            return true;
        }

        if (result && apiKey.Expiration.HasValue && apiKey.Expiration >= DateTime.UtcNow)
        {
            Task.Run(async () => await cache.RemoveAsync(key));
            return true;
        }

        return false;
    }

    public void AddApiKey(ApiKey key)
    {
        var options = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromMinutes(5))
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(15));
            
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(key, DistributedCacheExtensions.SerializerOptions));
        cache.Set(key.Key, bytes, options);
        
        var task = Task.Run(async () => await cache.SetAsync(key.Key, bytes, options));
        task.GetAwaiter().GetResult();
    }

    public async Task<bool> IsKeyValidAsync(string key)
    {
        var result = IsKeyValid(key);
        return await Task.FromResult(result);
    }

    public async Task<bool> AddApiKeyAsync(ApiKey key)
    {
        AddApiKey(key);
        return await Task.FromResult(true);
    }

    public async Task<int> DeleteExpiredKeysAsync()
    {
        return await Task.FromResult(0);
    }
}