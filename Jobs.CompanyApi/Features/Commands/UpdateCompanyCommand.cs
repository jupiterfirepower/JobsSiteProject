using Jobs.CompanyApi.DTOModels;
using MediatR;

namespace Jobs.CompanyApi.Features.Commands;

public record UpdateCompanyCommand(CompanyInDto Company) : IRequest<int>;