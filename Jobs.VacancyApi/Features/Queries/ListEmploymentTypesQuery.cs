using Jobs.Entities.DTO;
using MediatR;

namespace Jobs.VacancyApi.Features.Queries;

public record  ListEmploymentTypesQuery : IRequest<List<EmploymentTypeDto>>;