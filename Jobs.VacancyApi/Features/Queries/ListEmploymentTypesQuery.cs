using Jobs.DTO;
using MediatR;

namespace Jobs.VacancyApi.Features.Queries;

public record  ListEmploymentTypesQuery : IRequest<List<EmploymentTypeDto>>;