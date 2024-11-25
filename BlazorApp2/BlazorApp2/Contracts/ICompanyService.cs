using Jobs.Entities.DataModel;
using Jobs.Entities.DTO;

namespace BlazorApp2.Contracts;

public interface ICompanyService
{
    Task<CompanyDataInDto> AddCompanyAsync(string name, string note, string logoPath, string link);
    Task<bool> UpdateCompanyAsync(CompanyDataInDto company);
    Task<CompanyDto> GetCompanyByIdAsync(int companyId);
}