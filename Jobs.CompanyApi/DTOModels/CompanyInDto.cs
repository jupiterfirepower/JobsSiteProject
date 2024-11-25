using Jobs.CompanyApi.Validators;

namespace Jobs.CompanyApi.DTOModels;

public record CompanyInDto( int CompanyId, 
                            string CompanyName, 
                            string CompanyDescription,
                            string CompanyLogoPath,
                            string CompanyLink,
                            bool IsVisible = true,
                            bool IsActive = true) 
{
    public bool IsValid() => new CompanyInDtoValidator().Validate(this).IsValid;
}

