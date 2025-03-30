using Jobs.DTO;
using Jobs.ReferenceApi.Contracts;
using Jobs.ReferenceApi.Features.Queries;
using MediatR;

namespace Jobs.ReferenceApi.Features.Handlers;

public class GetWorkTypeQueryHandler(IProcessingService service) : IRequestHandler<GetWorkTypeQuery, WorkTypeDto>
{
    public async Task<WorkTypeDto> Handle(GetWorkTypeQuery request, CancellationToken cancellationToken) => await service.GetWorkTypeByIdAsync(request.Id);
}