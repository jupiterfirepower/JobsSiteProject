using Jobs.Common.Helpers;

namespace Jobs.CompanyApi.DTOModels.Helpers;

public static class SanitizerDtoHelper
{
    public static CompanyInDto SanitizeCompanyInDto(CompanyInDto entity)
    {
        var result = new CompanyInDto(entity.CompanyId,
            HtmlSanitizerHelper.Sanitize(entity.CompanyName),
            HtmlSanitizerHelper.Sanitize(entity.CompanyDescription),
            entity.CompanyLogoPath,
            entity.CompanyLink,
            entity.IsActive,
            entity.IsVisible);
       
        return result;
    }
}