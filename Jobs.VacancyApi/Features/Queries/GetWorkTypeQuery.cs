using Jobs.Entities.DTO;
using MediatR;

namespace Jobs.VacancyApi.Features.Queries;

public record GetWorkTypeQuery(int Id): IRequest<WorkTypeDto>;
