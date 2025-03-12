using System.Globalization;
using FirebirdSql.Data.FirebirdClient;
using Jobs.Core.Contracts.Providers;
using Jobs.Core.DataModel;

namespace Jobs.Core.Providers;

public class FirebirdApiKeyStorageServiceProvider: IApiKeyStorageServiceProvider
{
    
    private const string FirebirdConnectionString = "User=sysdba;Password=mkey;Database=apikeys.fdb;ServerType=1;"; 
    // FbServerType.Embedded = 1
    
    public bool IsKeyValid(string key)
    {
        var connection = new FbConnection(FirebirdConnectionString);
        connection.Open();
        
        if( !File.Exists( "apikeys.fdb") )
            FbConnection.CreateDatabase(FirebirdConnectionString);
        
        using var commandGetExpired = connection.CreateCommand();
        commandGetExpired.CommandText = "SELECT Expired, Count(*) as Count FROM ApiKeys WHERE ApiKey = @ApiKey;";
        commandGetExpired.Parameters.AddWithValue("@ApiKey", key);
        string expiredValue = null;
        long countValue = 0;

        using var reader = commandGetExpired.ExecuteReader();
        while (reader.Read())
        {
            if (reader["Expired"].GetType() != typeof(DBNull))
            {
                expiredValue = reader["Expired"].ToString();
                countValue = (long)reader["Count"];
            }
            else
            {
                expiredValue = null;
                countValue = (long)reader["Count"];
            }
        }
        
        if(countValue > 0 && expiredValue == null)
        {
            return true;
        }
        
        DateTime expiredDateTime = DateTime.ParseExact(expiredValue, "yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture);

        if (DateTime.UtcNow <= expiredDateTime)
        {
            using var commandDelete = connection.CreateCommand();
            commandDelete.CommandText = "DELETE FROM ApiKeys WHERE ApiKey = @ApiKey;";
            commandDelete.Parameters.AddWithValue("@ApiKey", key);
            var rowsDeleted = commandDelete.ExecuteScalar();
            Console.WriteLine($"Rows Deleted = {rowsDeleted}");
            
            return true;
        }
        
        return false;
    }

    public void AddApiKey(ApiKey key)
    {
        using var connection = new FbConnection(FirebirdConnectionString);
        connection.Open();
            
        using var commandInsert = connection.CreateCommand();
        commandInsert.CommandText = @"INSERT INTO ApiKeys (Id,ApiKey,Expired) VALUES (NULL,@ApiKey,@Expired)";
        //Values added to parameters
        commandInsert.Parameters.AddWithValue("@ApiKey", key);
        commandInsert.Parameters.AddWithValue("@Expired", key.Expiration == null ? DBNull.Value : key.Expiration);
          
        var rowsChanged = commandInsert.ExecuteNonQuery();
        Console.WriteLine($"Rows Changed = {rowsChanged}");
        commandInsert.ExecuteNonQuery();
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
        await using var connection = new FbConnection(FirebirdConnectionString);
        connection.Open();
            
        await using var commandDelete = connection.CreateCommand();
        commandDelete.CommandText = "DELETE FROM ApiKeys WHERE Expired <= datetime('now');"; // <
        var rowsDeleted = commandDelete.ExecuteNonQuery();
        Console.WriteLine($"Rows Deleted = {rowsDeleted}");
        return await Task.FromResult(rowsDeleted);
    }
}