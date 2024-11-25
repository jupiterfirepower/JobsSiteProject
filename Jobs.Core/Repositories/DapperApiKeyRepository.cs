using Dapper;
using Jobs.Core.Contracts;
using Jobs.Core.Contracts.Repositories;
using Jobs.Core.DataModel;
using Npgsql;

namespace Jobs.Core.Repositories;

public class DapperApiKeyRepository(string connectionString) : IApiKeyStoreRepository, IDisposable
{
    private readonly NpgsqlConnection _connection = new (connectionString); // ;Pooling=true;Minimum Pool Size=0;Maximum Pool Size=20;"
    private const string AddCommandText = "call sp_add_apikeys(@key,@expired);";
    private const string SelectCommandText = "select key as \"Key\", expired as \"Expiration\" from fn_get_apikey(@key);";
    private const string DeleteCommandText = "call sp_del_apikey(@key);";
    
    public void Add(ApiKey item)
    {
        //using var connection = new NpgsqlConnection(connectionString);
        
        try
        {
            _connection.Open();
            
            var queryArguments = new
            {
                key = item.Key,
                expired = item.Expiration
            };

            _connection.Execute(AddCommandText, queryArguments);
        }
        finally
        {
            _connection.Close();
        }
    }

    public async Task AddAsync(ApiKey item)
    {
        //await using var connection = new NpgsqlConnection(connectionString);
        
        try
        {
            await _connection.OpenAsync();
            
            var queryArguments = new
            {
                key = item.Key,
                expired = item.Expiration
            };

            await _connection.ExecuteAsync(AddCommandText, queryArguments);
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    public ApiKey Get(string key)
    {
        // using var connection = new NpgsqlConnection(connectionString);
        
        try
        {
            _connection.Open();
            var queryArgs = new { key };
            return _connection.QueryFirstOrDefault<ApiKey>(SelectCommandText, queryArgs);
        }
        finally
        {
            _connection.Close();
        }
    }

    public async Task<ApiKey> GetAsync(string key)
    {
        // await using var connection = new NpgsqlConnection(connectionString);
        
        try
        {
            await _connection.OpenAsync();
            var queryArgs = new { key };
            return await _connection.QueryFirstOrDefaultAsync<ApiKey>(SelectCommandText, queryArgs);
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }

    public void Remove(string key)
    {
        //using var connection = new NpgsqlConnection(connectionString);
        
        try
        {
            _connection.Open();
            
            var queryArgs = new { key = key };

            _connection.Execute(DeleteCommandText, queryArgs);
        }
        finally
        {
            _connection.Close();
        }
    }
    
    public async Task RemoveAsync(string key)
    {
        // await using var connection = new NpgsqlConnection(connectionString);
        try
        {
            await _connection.OpenAsync();
            
            var queryArgs = new { key = key };

            await _connection.ExecuteAsync(DeleteCommandText, queryArgs);
        }
        finally
        {
            await _connection.CloseAsync();
        }
    }
    
    private bool _disposed = false;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _connection.Dispose();
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