using Jobs.Common.Contracts;
using Jobs.Core.DataModel;
using Jobs.Core.Repositories;
using Jobs.Entities.Models;
using Jobs.VacancyApi.Data;

namespace JobsWebApiNUnitTests;

public class UnqliteApiKeyRepositoryUnitTests
{
    [Test]
    public void RepositoryAddApiKeyTest()
    {
        using var repository = new UnqliteApiKeyRepository();
        repository.Add(new ApiKey {Key = "12345", Expiration = DateTime.Now.AddHours(1)});
        Assert.Pass();
    }
    
    [Test]
    public void RepositoryAddAndGetApiKeyTest()
    {
        var apiKey = new ApiKey { Key = "12345", Expiration = DateTime.Now.AddHours(1) };
        using var repository = new UnqliteApiKeyRepository();
        repository.Add(apiKey);
        Assert.Pass();
        var data = repository.Get(apiKey.Key);
        Assert.IsNotNull(data);
        Assert.IsTrue(data.Key.Equals(apiKey.Key));
    }
    
    [Test]
    public void RepositoryAddAndRemoveApiKeyTest()
    {
        var apiKey = new ApiKey { Key = "12345", Expiration = DateTime.Now.AddHours(1) };
        using var repository = new UnqliteApiKeyRepository();
        repository.Add(apiKey);
        Assert.Pass();
        var data = repository.Get(apiKey.Key);
        Assert.IsNotNull(data);
        Assert.IsTrue(data.Key.Equals(apiKey.Key));
        repository.Remove(apiKey.Key);
        
        var removed = repository.Get(apiKey.Key);
        Assert.IsNull(removed);
    }
    
    [Test]
    public void RepositoryGetAllApiKeyTest()
    {
        var apiKey = new ApiKey { Key = "12345", Expiration = DateTime.Now.AddHours(1) };
        using var repository = new UnqliteApiKeyRepository();
        repository.Add(apiKey);
        Assert.Pass();
        var data = repository.Get(apiKey.Key);
        Assert.IsNotNull(data);
        Assert.IsTrue(data.Key.Equals(apiKey.Key));
        var apiKey1 = new ApiKey { Key = "123456", Expiration = DateTime.Now.AddHours(1) };
        repository.Add(apiKey1);

        var dataList = repository.GetAllAsync().Result;
        Assert.IsTrue(dataList.Count==2);
    }
}