using System.Text.Json;
using Jobs.Core.Contracts.Repositories;
using Jobs.Core.DataModel;
using UnQLiteNet;

namespace Jobs.Core.Repositories;

public class UnqliteApiKeyRepository: IApiKeyStoreRepositoryExtended, IDisposable
{
    private readonly UnQLite _db = new UnQLite(":mem:", UnQLiteOpenModel.Create | UnQLiteOpenModel.ReadWrite);
    
    public void Add(ApiKey item)
    {
        string jsonString = JsonSerializer.Serialize(item);
        _db.Save(item.Key, jsonString);  
        //_db.Append();
    }

    public async Task AddAsync(ApiKey item) => await Task.Run(() => Add(item));

    public ApiKey Get(string key)
    {
        var resultJson = _db.Get(key);
        var apiKey = resultJson != null ? JsonSerializer.Deserialize<ApiKey>(resultJson) : null;
        return apiKey;
    }

    public async Task<ApiKey> GetAsync(string key) => await Task.Run(() => Get(key));

    public void Remove(string key)
    {
        _db.Remove(key);
    }

    public async Task RemoveAsync(string key) => await Task.Run(() => Remove(key));

    public async Task<List<ApiKey>> GetAllAsync() => await Task.Run(GetAll);
    
    private List<ApiKey> GetAll()
    {
        var dataList = _db.GetAll();
        var result = dataList.Select(x =>
        {
            var apiKey = JsonSerializer.Deserialize<ApiKey>(x.Item2);
            return apiKey;
        });
        
        return result.ToList();
    }

    private bool _disposed = false;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _db.Close();
            }
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}