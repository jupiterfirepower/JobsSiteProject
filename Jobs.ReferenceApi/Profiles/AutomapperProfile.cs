using AutoMapper;
using Jobs.DTO;
using Jobs.Entities.Models;

namespace Jobs.ReferenceApi.Profiles;

public class AutomapperProfile : Profile
{
    public AutomapperProfile()
    {
        CreateMap<WorkTypeDto, WorkType>().ReverseMap();
        
        CreateMap<EmploymentTypeDto, EmploymentType>().ReverseMap();
        
        CreateMap<CategoryDto, Category>().ReverseMap();
    }
}