using Jobs.DTO;
using MediatR;

namespace Jobs.ReferenceApi.Features.Queries;

public record GetVacancyQuery(int Id): IRequest<VacancyDto>;
