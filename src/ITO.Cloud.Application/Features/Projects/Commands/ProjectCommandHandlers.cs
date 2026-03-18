using AutoMapper;
using ITO.Cloud.Application.Common.Exceptions;
using ITO.Cloud.Application.Features.Projects.DTOs;
using ITO.Cloud.Domain.Entities.Projects;
using ITO.Cloud.Domain.Interfaces;
using ITO.Cloud.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.Application.Features.Projects.Commands;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ProjectDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public CreateProjectCommandHandler(IApplicationDbContext db, IMapper mapper, ICurrentUserService currentUser)
        => (_db, _mapper, _currentUser) = (db, mapper, currentUser);

    public async Task<ProjectDto> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var exists = await _db.Projects.AnyAsync(p => p.Code == request.Code, cancellationToken);
        if (exists) throw new ConflictException($"Ya existe un proyecto con código '{request.Code}'.");

        var project = new Project
        {
            TenantId           = _currentUser.TenantId,
            CompanyId          = request.CompanyId,
            Code               = request.Code,
            Name               = request.Name,
            ProjectType        = request.ProjectType,
            Description        = request.Description,
            Address            = request.Address,
            City               = request.City,
            Region             = request.Region,
            StartDate          = request.StartDate,
            EstimatedEndDate   = request.EstimatedEndDate,
            TotalUnits         = request.TotalUnits,
            ItoManagerId       = request.ItoManagerId,
            MandanteName       = request.MandanteName,
            MandanteEmail      = request.MandanteEmail,
            ConstructionPermit = request.ConstructionPermit,
            Notes              = request.Notes,
            CreatedBy          = _currentUser.UserId
        };

        _db.Projects.Add(project);
        await _db.SaveChangesAsync(cancellationToken);

        var created = await _db.Projects
            .Include(p => p.Company)
            .FirstAsync(p => p.Id == project.Id, cancellationToken);
        return _mapper.Map<ProjectDto>(created);
    }
}

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ProjectDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public UpdateProjectCommandHandler(IApplicationDbContext db, IMapper mapper) => (_db, _mapper) = (db, mapper);

    public async Task<ProjectDto> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.Include(p => p.Company)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Proyecto", request.Id);

        project.Code             = request.Code;
        project.Name             = request.Name;
        project.ProjectType      = request.ProjectType;
        project.Status           = request.Status;
        project.Description      = request.Description;
        project.Address          = request.Address;
        project.City             = request.City;
        project.Region           = request.Region;
        project.StartDate        = request.StartDate;
        project.EstimatedEndDate = request.EstimatedEndDate;
        project.TotalUnits       = request.TotalUnits;
        project.ItoManagerId     = request.ItoManagerId;
        project.MandanteName     = request.MandanteName;
        project.IsActive         = request.IsActive;
        project.Notes            = request.Notes;

        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<ProjectDto>(project);
    }
}

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteProjectCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Proyecto", request.Id);

        project.IsDeleted = true;
        project.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }
}
