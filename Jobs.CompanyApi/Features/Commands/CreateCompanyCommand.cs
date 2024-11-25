using Jobs.CompanyApi.DTOModels;
using MediatR;

namespace Jobs.CompanyApi.Features.Commands;

public record CreateCompanyCommand(CompanyInDto Company) : IRequest<CompanyDto>;