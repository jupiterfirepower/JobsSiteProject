using Jobs.Entities.DataModel;
using Jobs.Entities.DTO;

namespace BlazorApp2.Contracts.Clients;

public interface ICompanyClientService
{
    Task<CompanyDataInDto> AddCompanyAsync(CompanyDataInDto company);
    Task<bool> UpdateCompanyAsync(CompanyDataInDto company);

    Task<CompanyDto> GetCompanyByIdAsync(int companyId);
}