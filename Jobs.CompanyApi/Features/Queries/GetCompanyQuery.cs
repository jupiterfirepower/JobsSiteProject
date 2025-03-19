using Jobs.Entities.DTO;
using MediatR;

namespace Jobs.CompanyApi.Features.Queries;

public record GetCompanyQuery(int Id): IRequest<CompanyDto>;