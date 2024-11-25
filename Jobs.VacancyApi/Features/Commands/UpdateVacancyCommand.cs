using Jobs.Entities.DTO;
using MediatR;

namespace Jobs.VacancyApi.Features.Commands;

public record UpdateVacancyCommand(VacancyInDto Vacancy) : IRequest<int>;
