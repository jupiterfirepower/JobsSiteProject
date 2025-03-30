using AutoMapper;
using Jobs.Common.Contracts;
using Jobs.DTO;
using Jobs.Entities.Models;
using Jobs.ReferenceApi.Contracts;
using Jobs.ReferenceApi.Helpers;

namespace Jobs.ReferenceApi.Services;

public class ProcessingService(IGenericRepository<WorkType> workTypesRepository, 
    IGenericRepository<EmploymentType> empRepository, IGenericRepository<Category> categoryRepository, 
    ICacheService cacheService, IMapper mapper) : IProcessingService
{
    private List<TR> GetDataAsync<T, TR>(string fileName, Func<Task<List<T>>> fn)
    {
        if (File.Exists(fileName))
        {
            string jsonResult = File.ReadAllText(fileName);
            return DataSerializerHelper.Deserialize<TR>(jsonResult);
        }

        var result = GetDataFromDatabaseAsync<T, TR>(fn);
        SaveDataToFile(fileName, result);
        return result;
    }
    
    private List<TR> GetDataFromDatabaseAsync<T, TR>(Func<Task<List<T>>> fn)
    {
        var items = fn().Result;
        var result = mapper.Map<List<TR>>(items);
        return result;
    }
    
    private void SaveDataToFile<T>(string fileName, List<T> data) => DataSerializerHelper.Serialize(fileName, data);

    /*private async Task LoadDataToLocalCacheService()
    {
        var categories = await Task.FromResult(GetDataAsync<Category, CategoryDto>("Categories.json",async () => await categoryRepository.GetAllAsync()));
        cacheService.SetData("categories", categories, DateTimeOffset.UtcNow.AddYears(100));
        var empTypes = await Task.FromResult(GetDataAsync<EmploymentType, EmploymentTypeDto>("EmpTypes.json",async () => await empRepository.GetAllAsync()));
        cacheService.SetData("empTypes", empTypes, DateTimeOffset.UtcNow.AddYears(100));
        var workTypes = await Task.FromResult(GetDataAsync<WorkType, WorkTypeDto>("WorkTypes.json",async () => await workTypesRepository.GetAllAsync()));
        cacheService.SetData("workTypes", workTypes, DateTimeOffset.UtcNow.AddYears(100));
    }*/
    
    private async Task LoadCategoriesDataToLocalCacheService()
    {
        var categories = await Task.FromResult(GetDataAsync<Category, CategoryDto>("Categories.json",async () => await categoryRepository.GetAllAsync()));
        cacheService.SetData("categories", categories, DateTimeOffset.UtcNow.AddYears(100));
    }
    
    private async Task LoadEmploymentTypesDataToLocalCacheService()
    {
        var empTypes = await Task.FromResult(GetDataAsync<EmploymentType, EmploymentTypeDto>("EmpTypes.json",async () => await empRepository.GetAllAsync()));
        cacheService.SetData("empTypes", empTypes, DateTimeOffset.UtcNow.AddYears(100));
    }
    
    private async Task LoadWorkTypesDataToLocalCacheService()
    {
        var workTypes = await Task.FromResult(GetDataAsync<WorkType, WorkTypeDto>("WorkTypes.json",async () => await workTypesRepository.GetAllAsync()));
        cacheService.SetData("workTypes", workTypes, DateTimeOffset.UtcNow.AddYears(100));
    }
    
    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        await CheckOrLoadCategoriesData();
        
        return cacheService.GetData<List<CategoryDto>>("categories");
    }
    
    public async Task<List<EmploymentTypeDto>> GetEmploymentTypesAsync()
    {
        await CheckOrLoadEmploymentTypesData();
        
        return cacheService.GetData<List<EmploymentTypeDto>>("empTypes");
    }

    public async Task<List<WorkTypeDto>> GetWorkTypesAsync()
    {
        await CheckOrLoadWorkTypesData();
        
        return cacheService.GetData<List<WorkTypeDto>>("workTypes");
    }
    
    public async Task<EmploymentTypeDto> GetEmploymentTypeByIdAsync(int id)
    {
        await CheckOrLoadEmploymentTypesData();
        
        var currentListData = cacheService.GetData<List<EmploymentTypeDto>>("empTypes");
        var current = currentListData.FirstOrDefault(x=>x.EmploymentTypeId == id);

        return mapper.Map<EmploymentTypeDto>(current);
    }
    
    public async Task<WorkTypeDto> GetWorkTypeByIdAsync(int id)
    {
        await CheckOrLoadWorkTypesData();
        
        var currentListData = cacheService.GetData<List<WorkTypeDto>>("workTypes");
        var current = currentListData.FirstOrDefault(x=>x.WorkTypeId == id);

        return mapper.Map<WorkTypeDto>(current);
    }
    
    # region CheckAndLoad
    private async Task CheckOrLoadCategoriesData()
    {
        if (!cacheService.HasData("categories"))
            await LoadCategoriesDataToLocalCacheService();
    }

    private async Task CheckOrLoadWorkTypesData()
    {
        if (!cacheService.HasData("workTypes"))
            await LoadWorkTypesDataToLocalCacheService();
    }
    
    private async Task CheckOrLoadEmploymentTypesData()
    {
        if (!cacheService.HasData("empTypes"))
            await LoadEmploymentTypesDataToLocalCacheService();
    }
    # endregion

}