using Jobs.Core.Contracts.Providers;
using Jobs.Core.Contracts.Repositories;
using Jobs.Core.DataModel;

namespace Jobs.Core.Providers;

public class UnqliteApiKeyStorageServiceProvider(IApiKeyStoreRepositoryExtended repository) : IApiKeyStorageServiceProvider
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

    public void AddApiKey(ApiKey item) => repository.Add(item);

    public async Task<bool> IsKeyValidAsync(string key) => await Task.Run(() => IsKeyValid(key));

    public async Task<bool> AddApiKeyAsync(ApiKey item)
    {
        await Task.Run(() => AddApiKey(item));
        return true;
    }

    public async Task<int> DeleteExpiredKeysAsync()
    {
        return await Task.Run(async () => { 
            int count = 0;
        
            (await repository.GetAllAsync()).
                Where(x=> x.Expiration.HasValue && x.Expiration.Value < DateTime.UtcNow).
                ToList().
                ForEach(x=>
                {
                    repository.Remove(x.Key);
                    count++;
                });
            
            return count;
        });
    }
}