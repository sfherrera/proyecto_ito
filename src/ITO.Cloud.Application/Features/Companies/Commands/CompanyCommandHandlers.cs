using AutoMapper;
using ITO.Cloud.Application.Common.Exceptions;
using ITO.Cloud.Application.Features.Companies.DTOs;
using ITO.Cloud.Domain.Entities.Projects;
using ITO.Cloud.Domain.Interfaces;
using ITO.Cloud.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.Application.Features.Companies.Commands;

public class CreateCompanyCommandHandler : IRequestHandler<CreateCompanyCommand, CompanyDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public CreateCompanyCommandHandler(IApplicationDbContext db, IMapper mapper, ICurrentUserService currentUser)
        => (_db, _mapper, _currentUser) = (db, mapper, currentUser);

    public async Task<CompanyDto> Handle(CreateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = new Company
        {
            TenantId   = _currentUser.TenantId,
            Name       = request.Name,
            Rut        = request.Rut,
            BusinessName = request.BusinessName,
            CompanyType = request.CompanyType,
            Address    = request.Address,
            City       = request.City,
            Region     = request.Region,
            Phone      = request.Phone,
            Email      = request.Email,
            Website    = request.Website,
            Notes      = request.Notes,
            CreatedBy  = _currentUser.UserId
        };

        _db.Companies.Add(company);
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CompanyDto>(company);
    }
}

public class UpdateCompanyCommandHandler : IRequestHandler<UpdateCompanyCommand, CompanyDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public UpdateCompanyCommandHandler(IApplicationDbContext db, IMapper mapper) => (_db, _mapper) = (db, mapper);

    public async Task<CompanyDto> Handle(UpdateCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Empresa", request.Id);

        company.Name         = request.Name;
        company.Rut          = request.Rut;
        company.BusinessName = request.BusinessName;
        company.CompanyType  = request.CompanyType;
        company.Address      = request.Address;
        company.City         = request.City;
        company.Region       = request.Region;
        company.Phone        = request.Phone;
        company.Email        = request.Email;
        company.Website      = request.Website;
        company.IsActive     = request.IsActive;
        company.Notes        = request.Notes;

        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<CompanyDto>(company);
    }
}

public class DeleteCompanyCommandHandler : IRequestHandler<DeleteCompanyCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteCompanyCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteCompanyCommand request, CancellationToken cancellationToken)
    {
        var company = await _db.Companies.FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Empresa", request.Id);

        company.IsDeleted = true;
        company.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
