using Jobs.Entities.DTO;
using MediatR;

namespace Jobs.VacancyApi.Features.Queries;

public record  ListWorkTypesQuery : IRequest<List<WorkTypeDto>>;
