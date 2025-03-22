using Jobs.DTO;
using Jobs.ReferenceApi.Contracts;
using Jobs.ReferenceApi.Features.Queries;
using MediatR;

namespace Jobs.ReferenceApi.Features.Handlers;

public class ListEmploymentTypeQueryHandler(IProcessingService service) : IRequestHandler<ListEmploymentTypesQuery, List<EmploymentTypeDto>>
{
    public async Task<List<EmploymentTypeDto>> Handle(ListEmploymentTypesQuery request, CancellationToken cancellationToken) => 
        await service.GetEmploymentTypesAsync();
}