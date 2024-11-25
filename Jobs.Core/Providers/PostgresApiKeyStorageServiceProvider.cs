using Jobs.Core.Contracts;
using Jobs.Core.Contracts.Providers;
using Jobs.Core.Contracts.Repositories;
using Jobs.Core.DataModel;

namespace Jobs.Core.Providers;

public class PostgresApiKeyStorageServiceProvider(IApiKeyStoreRepository repository) : IApiKeyStorageServiceProvider
{
    public bool IsKeyValid(string key)
    {
        var current = repository.Get(key);
        
        if (current != null && current.Expiration >= DateTime.UtcNow)
        {
            return true;
        }

        return false;
    }

    public void AddApiKey(ApiKey item)
    {
        repository.Add(item);
    }

    public async Task<bool> IsKeyValidAsync(string key)
    {
        var current = await repository.GetAsync(key);
        
        if (current != null && current.Expiration >= DateTime.UtcNow)
        {
            return true;
        }

        return false;
    }

    public async Task<bool> AddApiKeyAsync(ApiKey item)
    {
        await repository.AddAsync(item);
        return true;
    }

    public Task<int> DeleteExpiredKeysAsync()
    {
        throw new NotImplementedException();
    }
}