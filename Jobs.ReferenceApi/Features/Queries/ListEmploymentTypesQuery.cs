using Jobs.DTO;
using MediatR;

namespace Jobs.ReferenceApi.Features.Queries;

public record  ListEmploymentTypesQuery : IRequest<List<EmploymentTypeDto>>;