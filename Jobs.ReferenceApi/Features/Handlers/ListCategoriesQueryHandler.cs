using Jobs.DTO;
using Jobs.ReferenceApi.Contracts;
using Jobs.ReferenceApi.Features.Queries;
using MediatR;

namespace Jobs.ReferenceApi.Features.Handlers;

public class ListCategoriesQueryHandler(IProcessingService service) : IRequestHandler<ListCategoriesQuery, List<CategoryDto>>
{
    public async Task<List<CategoryDto>> Handle(ListCategoriesQuery request, CancellationToken cancellationToken) =>
        await service.GetCategoriesAsync();
}