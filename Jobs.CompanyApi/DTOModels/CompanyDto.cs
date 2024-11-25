namespace Jobs.CompanyApi.DTOModels;

public record CompanyDto( int CompanyId, 
                          string CompanyName, 
                          string CompanyDescription,
                          string CompanyLogoPath,
                          string CompanyLink,
                          bool IsVisible = true,
                          bool IsActive = true, 
                          DateTime Created = default,
                          DateTime Modified = default );