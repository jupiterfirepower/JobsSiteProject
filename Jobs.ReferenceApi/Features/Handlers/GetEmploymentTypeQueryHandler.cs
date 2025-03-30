using Jobs.DTO;
using Jobs.ReferenceApi.Contracts;
using Jobs.ReferenceApi.Features.Queries;
using MediatR;

namespace Jobs.ReferenceApi.Features.Handlers;

public class GetEmploymentTypeQueryHandler(IProcessingService service) : IRequestHandler<GetEmploymentTypeQuery, EmploymentTypeDto>
{
    public async Task<EmploymentTypeDto> Handle(GetEmploymentTypeQuery request, CancellationToken cancellationToken) => 
        await service.GetEmploymentTypeByIdAsync(request.Id);
}