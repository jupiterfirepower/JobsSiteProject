using Jobs.Entities.DTO;
using MediatR;

namespace Jobs.VacancyApi.Features.Commands;

public record CreateVacancyCommand(VacancyInDto Vacancy) : IRequest<VacancyDto>;

