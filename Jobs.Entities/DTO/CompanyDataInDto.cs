namespace Jobs.Entities.DTO;

public record CompanyDataInDto(
    int CompanyId, 
    string CompanyName, 
    string CompanyDescription,
    string CompanyLogoPath,
    string CompanyLink,
    bool IsVisible = true,
    bool IsActive = true);