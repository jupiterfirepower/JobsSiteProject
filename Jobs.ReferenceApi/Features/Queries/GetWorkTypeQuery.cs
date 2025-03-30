using Jobs.DTO;
using MediatR;

namespace Jobs.ReferenceApi.Features.Queries;

public record GetWorkTypeQuery(int Id): IRequest<WorkTypeDto>;
