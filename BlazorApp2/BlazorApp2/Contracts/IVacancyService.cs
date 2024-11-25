using Jobs.Entities.DTO;

namespace BlazorApp2.Contracts;

public interface IVacancyService
{
    Task<List<WorkTypeDto>> GetWorkTypesAsync();
    Task<List<EmploymentTypeDto>> GetEmploymentTypesAsync();
    Task<List<CategoryDto>> GetCategoriesAsync();

    Task<bool> AddVacancyAsync(VacancyInDto vacancy);
}