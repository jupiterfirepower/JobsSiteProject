using Jobs.DTO;
using MediatR;

namespace Jobs.ReferenceApi.Features.Queries;

public record ListCategoriesQuery : IRequest<List<CategoryDto>>;