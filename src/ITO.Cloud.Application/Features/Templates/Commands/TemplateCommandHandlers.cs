using AutoMapper;
using ITO.Cloud.Application.Common.Exceptions;
using ITO.Cloud.Application.Common.Interfaces;
using ITO.Cloud.Application.Features.Templates.DTOs;
using ITO.Cloud.Domain.Entities.Templates;
using ITO.Cloud.Domain.Enums;
using ITO.Cloud.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.Application.Features.Templates.Commands;

public class CreateTemplateCommandHandler : IRequestHandler<CreateTemplateCommand, TemplateDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public CreateTemplateCommandHandler(IApplicationDbContext db, IMapper mapper, ICurrentUserService currentUser)
        => (_db, _mapper, _currentUser) = (db, mapper, currentUser);

    public async Task<TemplateDto> Handle(CreateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = new InspectionTemplate
        {
            TenantId            = _currentUser.TenantId,
            Name                = request.Name,
            Description         = request.Description,
            TemplateType        = request.TemplateType,
            SpecialtyId         = request.SpecialtyId,
            Status              = TemplateStatus.Borrador,
            CurrentVersion      = 1,
            IsGlobal            = request.IsGlobal,
            RequireGeolocation  = request.RequireGeolocation,
            RequireSignature    = request.RequireSignature,
            PassingScore        = request.PassingScore,
            CreatedBy           = _currentUser.UserId
        };

        _db.InspectionTemplates.Add(template);
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<TemplateDto>(template);
    }
}

public class UpdateTemplateCommandHandler : IRequestHandler<UpdateTemplateCommand, TemplateDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public UpdateTemplateCommandHandler(IApplicationDbContext db, IMapper mapper) => (_db, _mapper) = (db, mapper);

    public async Task<TemplateDto> Handle(UpdateTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _db.InspectionTemplates
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Plantilla", request.Id);

        template.Name               = request.Name;
        template.Description        = request.Description;
        template.Status             = request.Status;
        template.IsGlobal           = request.IsGlobal;
        template.RequireGeolocation = request.RequireGeolocation;
        template.RequireSignature   = request.RequireSignature;
        template.PassingScore       = request.PassingScore;

        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<TemplateDto>(template);
    }
}

public class PublishTemplateCommandHandler : IRequestHandler<PublishTemplateCommand, TemplateDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public PublishTemplateCommandHandler(IApplicationDbContext db, IMapper mapper) => (_db, _mapper) = (db, mapper);

    public async Task<TemplateDto> Handle(PublishTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _db.InspectionTemplates
            .IgnoreAutoIncludes()
            .Include(t => t.Sections)
                .ThenInclude(s => s.Questions)
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Plantilla", request.Id);

        if (!template.Sections.Any())
            throw new InvalidOperationException("No se puede publicar una plantilla sin secciones.");

        if (!template.Sections.Any(s => s.Questions.Any()))
            throw new InvalidOperationException("No se puede publicar una plantilla sin preguntas.");

        template.Status = TemplateStatus.Activa;

        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<TemplateDto>(template);
    }
}

public class DeleteTemplateCommandHandler : IRequestHandler<DeleteTemplateCommand>
{
    private readonly IApplicationDbContext _db;

    public DeleteTemplateCommandHandler(IApplicationDbContext db) => _db = db;

    public async Task Handle(DeleteTemplateCommand request, CancellationToken cancellationToken)
    {
        var template = await _db.InspectionTemplates
            .FirstOrDefaultAsync(t => t.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Plantilla", request.Id);

        template.IsDeleted = true;
        template.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
    }
}

public class AddTemplateSectionCommandHandler : IRequestHandler<AddTemplateSectionCommand, TemplateSectionDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public AddTemplateSectionCommandHandler(IApplicationDbContext db, IMapper mapper, ICurrentUserService currentUser)
        => (_db, _mapper, _currentUser) = (db, mapper, currentUser);

    public async Task<TemplateSectionDto> Handle(AddTemplateSectionCommand request, CancellationToken cancellationToken)
    {
        var templateExists = await _db.InspectionTemplates
            .AnyAsync(t => t.Id == request.TemplateId, cancellationToken);

        if (!templateExists)
            throw new NotFoundException("Plantilla", request.TemplateId);

        var section = new TemplateSection
        {
            TenantId    = _currentUser.TenantId,
            TemplateId  = request.TemplateId,
            Title       = request.Title,
            Description = request.Description,
            OrderIndex  = request.OrderIndex,
            Weight      = request.Weight,
            CreatedBy   = _currentUser.UserId
        };

        _db.TemplateSections.Add(section);
        await _db.SaveChangesAsync(cancellationToken);
        return _mapper.Map<TemplateSectionDto>(section);
    }
}

public class AddTemplateQuestionCommandHandler : IRequestHandler<AddTemplateQuestionCommand, TemplateQuestionDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public AddTemplateQuestionCommandHandler(IApplicationDbContext db, IMapper mapper, ICurrentUserService currentUser)
        => (_db, _mapper, _currentUser) = (db, mapper, currentUser);

    public async Task<TemplateQuestionDto> Handle(AddTemplateQuestionCommand request, CancellationToken cancellationToken)
    {
        var sectionExists = await _db.TemplateSections
            .AnyAsync(s => s.Id == request.SectionId, cancellationToken);

        if (!sectionExists)
            throw new NotFoundException("Sección de plantilla", request.SectionId);

        var question = new TemplateQuestion
        {
            TenantId         = _currentUser.TenantId,
            SectionId        = request.SectionId,
            ParentQuestionId = request.ParentQuestionId,
            TriggerValue     = request.TriggerValue,
            QuestionText     = request.QuestionText,
            Description      = request.Description,
            QuestionType     = request.QuestionType,
            OrderIndex       = request.OrderIndex,
            IsRequired       = request.IsRequired,
            IsCritical       = request.IsCritical,
            Weight           = request.Weight,
            MinValue         = request.MinValue,
            MaxValue         = request.MaxValue,
            MinPhotos        = request.MinPhotos,
            MaxPhotos        = request.MaxPhotos,
            CreatedBy        = _currentUser.UserId
        };

        _db.TemplateQuestions.Add(question);
        await _db.SaveChangesAsync(cancellationToken);

        // Add options if provided
        if (request.Options is { Count: > 0 })
        {
            foreach (var opt in request.Options)
            {
                _db.TemplateQuestionOptions.Add(new TemplateQuestionOption
                {
                    QuestionId      = question.Id,
                    Label           = opt.Label,
                    Value           = opt.Value,
                    OrderIndex      = opt.OrderIndex,
                    IsFailureOption = opt.IsFailureOption,
                    Score           = opt.Score
                });
            }
            await _db.SaveChangesAsync(cancellationToken);
        }

        // Reload with options for the response
        var result = await _db.TemplateQuestions
            .AsNoTracking()
            .Include(q => q.Options.OrderBy(o => o.OrderIndex))
            .FirstAsync(q => q.Id == question.Id, cancellationToken);

        return _mapper.Map<TemplateQuestionDto>(result);
    }
}
