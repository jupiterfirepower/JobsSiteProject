using Jobs.DTO;
using Jobs.ReferenceApi.Contracts;
using Jobs.ReferenceApi.Features.Queries;
using MediatR;

namespace Jobs.ReferenceApi.Features.Handlers;

public class ListWorkTypesQueryHandler(IProcessingService service) : IRequestHandler<ListWorkTypesQuery, List<WorkTypeDto>>
{
    public async Task<List<WorkTypeDto>> Handle(ListWorkTypesQuery request, CancellationToken cancellationToken) =>
        await service.GetWorkTypesAsync();
}