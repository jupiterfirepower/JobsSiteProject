using Jobs.Core.Contracts.Providers;
using DuckDB.NET.Data;
using Jobs.Core.DataModel;

namespace Jobs.Core.Providers;

public class DuckDbApiKeyStorageServiceProvider : IApiKeyStorageServiceProvider, IDisposable, IAsyncDisposable
{
    private readonly DuckDBConnection _duckDbConnection = new("Data Source=:memory:");
    
    public DuckDbApiKeyStorageServiceProvider()
    {
        _duckDbConnection.Open();
        
        using var command = _duckDbConnection.CreateCommand();
        command.CommandText = "CREATE TABLE apikeys (id INTEGER PRIMARY KEY, apikey Text NOT NULL, expired TIMESTAMP DEFAULT NULL);";
        command.ExecuteNonQuery();
        
        using var commandSequence = _duckDbConnection.CreateCommand();
        commandSequence.CommandText = "CREATE SEQUENCE serial START WITH 1 INCREMENT BY 1;";
        commandSequence.ExecuteNonQuery();
    }
    
    public bool IsKeyValid(string key)
    {
        Console.WriteLine($"Key = {key}");
        using var commandGetCount = _duckDbConnection.CreateCommand();
        commandGetCount.CommandText = commandGetCount.CommandText = 
            "SELECT expired, Count(*) FROM apikeys WHERE apikey = $ApiKey GROUP BY expired;"; 
        
        //Values added to parameters
        commandGetCount.Parameters.Add(new DuckDBParameter("ApiKey", key));
        
        DateTime? expiredValue = null;
        long countValue = 0;
            
        var reader = commandGetCount.ExecuteReader();
        while (reader.Read())
        {
            expiredValue = reader.GetDateTime(0);
            countValue = reader.GetInt64(1);
        }
       
        if(countValue > 0 && expiredValue == null)
        {
            return true;
        }
        
        if (expiredValue.HasValue && DateTime.Now <= expiredValue.Value)
        {
            using var commandDelete = _duckDbConnection.CreateCommand();
            commandDelete.CommandText = "DELETE FROM apikeys WHERE apikey = $ApiKey;";
            commandDelete.Parameters.Add(new DuckDBParameter("ApiKey", key));
            var rowsDeleted = commandDelete.ExecuteNonQuery();
            Console.WriteLine($"Rows Deleted = {rowsDeleted}");
            
            return true;
        }
        
        return false;
    }

    public void AddApiKey(ApiKey key)
    {
        using var commandInsert = _duckDbConnection.CreateCommand();
        commandInsert.CommandText = "INSERT INTO apikeys (id,apikey,expired) VALUES (nextval('serial'),$ApiKey,$Expired)";
        //Values added to parameters
        commandInsert.Parameters.Add(new DuckDBParameter("ApiKey", key.Key));
        commandInsert.Parameters.Add(new DuckDBParameter("Expired", key.Expiration == null ? DBNull.Value : key.Expiration));
          
        var rowsChanged = commandInsert.ExecuteNonQuery();
        Console.WriteLine($"Rows Changed = {rowsChanged}");
    }

    public async Task<bool> IsKeyValidAsync(string key)
    {
        IsKeyValid(key);
        return await Task.FromResult(true);
    }

    public async Task<bool> AddApiKeyAsync(ApiKey key)
    {
        AddApiKey(key);
        return await Task.FromResult(true);
    }

    public async Task<int> DeleteExpiredKeysAsync()
    {
        await using var commandDelete = _duckDbConnection.CreateCommand();
        commandDelete.CommandText = "DELETE FROM apikeys WHERE Expired <= now();"; // <
        var rowsDeleted = commandDelete.ExecuteNonQuery();
        Console.WriteLine($"Rows Deleted = {rowsDeleted}");
        return await Task.FromResult(rowsDeleted);
    }
    
    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _duckDbConnection?.Dispose();
            }
        }
        _disposed = true;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync() => await DisposeAsync(true);
    
    private async ValueTask DisposeAsync(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (_duckDbConnection != null) await _duckDbConnection.DisposeAsync();
            }
        }
        _disposed = true;
    }
}