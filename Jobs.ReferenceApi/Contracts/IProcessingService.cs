using Jobs.DTO;
using Jobs.DTO.In;

namespace Jobs.ReferenceApi.Contracts;

public interface IProcessingService
{
    Task<List<EmploymentTypeDto>> GetEmploymentTypesAsync();
    Task<EmploymentTypeDto> GetEmploymentTypeByIdAsync(int id);
    Task<List<CategoryDto>> GetCategoriesAsync();
    Task<List<WorkTypeDto>> GetWorkTypesAsync();
    Task<WorkTypeDto> GetWorkTypeByIdAsync(int id);
}