using Jobs.DTO;
using MediatR;

namespace Jobs.ReferenceApi.Features.Queries;

public record GetEmploymentTypeQuery(int Id): IRequest<EmploymentTypeDto>;
