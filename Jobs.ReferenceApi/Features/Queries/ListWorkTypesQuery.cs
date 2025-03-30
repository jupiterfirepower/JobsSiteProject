using Jobs.DTO;
using MediatR;

namespace Jobs.ReferenceApi.Features.Queries;

public record  ListWorkTypesQuery : IRequest<List<WorkTypeDto>>;
