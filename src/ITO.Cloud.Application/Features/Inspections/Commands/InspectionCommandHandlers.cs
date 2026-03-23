using AutoMapper;
using ITO.Cloud.Application.Common.Exceptions;
using ITO.Cloud.Application.Features.Inspections.DTOs;
using ITO.Cloud.Domain.Entities.Identity;
using ITO.Cloud.Domain.Entities.Inspections;
using ITO.Cloud.Domain.Entities.Observations;
using ITO.Cloud.Domain.Enums;
using ITO.Cloud.Domain.Interfaces;
using ITO.Cloud.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.Application.Features.Inspections.Commands;

public class CreateInspectionCommandHandler : IRequestHandler<CreateInspectionCommand, InspectionDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public CreateInspectionCommandHandler(IApplicationDbContext db, IMapper mapper, ICurrentUserService currentUser)
        => (_db, _mapper, _currentUser) = (db, mapper, currentUser);

    public async Task<InspectionDto> Handle(CreateInspectionCommand request, CancellationToken cancellationToken)
    {
        // Generar correlativo
        var year = DateTime.UtcNow.Year;
        var counter = await _db.SequenceCounters
            .FirstOrDefaultAsync(s => s.EntityType == "inspection" && s.Year == year, cancellationToken);

        if (counter == null)
        {
            counter = new SequenceCounter
            {
                TenantId = _currentUser.TenantId, EntityType = "inspection",
                Prefix = "INS", Year = year, LastValue = 1
            };
            _db.SequenceCounters.Add(counter);
        }
        else counter.LastValue++;

        var code = $"INS-{year}-{counter.LastValue:D4}";

        // Contar preguntas del template
        var questionCount = await _db.TemplateQuestions
            .Where(q => q.Section.TemplateId == request.TemplateId && q.IsActive)
            .CountAsync(cancellationToken);

        var inspection = new Inspection
        {
            TenantId         = _currentUser.TenantId,
            ProjectId        = request.ProjectId,
            TemplateId       = request.TemplateId,
            Code             = code,
            Title            = request.Title,
            InspectionType   = request.InspectionType,
            Priority         = request.Priority,
            ScheduledDate    = DateTime.SpecifyKind(request.ScheduledDate, DateTimeKind.Utc),
            ScheduledEndDate = request.ScheduledEndDate.HasValue ? DateTime.SpecifyKind(request.ScheduledEndDate.Value, DateTimeKind.Utc) : null,
            StageId          = request.StageId,
            SectorId         = request.SectorId,
            UnitId           = request.UnitId,
            AssignedToId     = request.AssignedToId,
            AssignedById     = _currentUser.UserId,
            ContractorId     = request.ContractorId,
            SpecialtyId      = request.SpecialtyId,
            Description      = request.Description,
            TotalQuestions   = questionCount,
            CreatedBy        = _currentUser.UserId
        };

        _db.Inspections.Add(inspection);
        await _db.SaveChangesAsync(cancellationToken);
        return new InspectionDto(
            inspection.Id, inspection.ProjectId, inspection.TemplateId, inspection.Code, inspection.Title,
            inspection.InspectionType, inspection.Status, inspection.Priority,
            inspection.ScheduledDate, inspection.StartedAt, inspection.FinishedAt,
            inspection.AssignedToId, null,
            inspection.ContractorId, null,
            inspection.StageId, inspection.SectorId, inspection.UnitId,
            inspection.Score, inspection.Passed,
            inspection.TotalQuestions, inspection.AnsweredQuestions,
            inspection.ConformingCount, inspection.NonConformingCount,
            inspection.IsOfflineCreated, inspection.Notes, inspection.CreatedAt);
    }
}

public class StartInspectionCommandHandler : IRequestHandler<StartInspectionCommand, InspectionDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;

    public StartInspectionCommandHandler(IApplicationDbContext db, IMapper mapper) => (_db, _mapper) = (db, mapper);

    public async Task<InspectionDto> Handle(StartInspectionCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _db.Inspections.FirstOrDefaultAsync(i => i.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException("Inspección", request.Id);

        if (inspection.Status != InspectionStatus.Programada)
            throw new ConflictException("Solo se pueden iniciar inspecciones en estado Programada.");

        inspection.Status    = InspectionStatus.EnProceso;
        inspection.StartedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);
        return new InspectionDto(
            inspection.Id, inspection.ProjectId, inspection.TemplateId, inspection.Code, inspection.Title,
            inspection.InspectionType, inspection.Status, inspection.Priority,
            inspection.ScheduledDate, inspection.StartedAt, inspection.FinishedAt,
            inspection.AssignedToId, null, inspection.ContractorId, null,
            inspection.StageId, inspection.SectorId, inspection.UnitId,
            inspection.Score, inspection.Passed,
            inspection.TotalQuestions, inspection.AnsweredQuestions,
            inspection.ConformingCount, inspection.NonConformingCount,
            inspection.IsOfflineCreated, inspection.Notes, inspection.CreatedAt);
    }
}

public class SubmitInspectionCommandHandler : IRequestHandler<SubmitInspectionCommand, InspectionDto>
{
    private readonly IApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUser;

    public SubmitInspectionCommandHandler(IApplicationDbContext db, IMapper mapper, ICurrentUserService currentUser)
        => (_db, _mapper, _currentUser) = (db, mapper, currentUser);

    public async Task<InspectionDto> Handle(SubmitInspectionCommand request, CancellationToken cancellationToken)
    {
        var inspection = await _db.Inspections
            .Include(i => i.Answers)
            .FirstOrDefaultAsync(i => i.Id == request.InspectionId, cancellationToken)
            ?? throw new NotFoundException("Inspección", request.InspectionId);

        if (inspection.Status == InspectionStatus.Cerrada)
            throw new ConflictException("La inspección ya está cerrada.");

        var now = DateTime.UtcNow;

        // Guardar respuestas (upsert)
        foreach (var answerDto in request.Answers)
        {
            var existing = inspection.Answers.FirstOrDefault(a => a.QuestionId == answerDto.QuestionId);
            if (existing == null)
            {
                existing = new InspectionAnswer
                {
                    TenantId     = inspection.TenantId,
                    InspectionId = inspection.Id,
                    QuestionId   = answerDto.QuestionId,
                };
                _db.InspectionAnswers.Add(existing);
            }

            existing.AnswerValue      = answerDto.AnswerValue;
            existing.SelectedOptionId = answerDto.SelectedOptionId;
            existing.NumericValue     = answerDto.NumericValue;
            existing.DateValue        = answerDto.DateValue;
            existing.IsNa             = answerDto.IsNa;
            existing.Notes            = answerDto.Notes;
            existing.AnsweredAt       = now;
            existing.AnsweredBy       = _currentUser.UserId;

            // Determinar conformidad automática
            if (!answerDto.IsNa && answerDto.SelectedOptionId.HasValue)
            {
                var option = await _db.TemplateQuestionOptions
                    .FirstOrDefaultAsync(o => o.Id == answerDto.SelectedOptionId, cancellationToken);
                if (option != null)
                    existing.IsConforming = !option.IsFailureOption;
            }
        }

        // Recalcular estadísticas
        await _db.SaveChangesAsync(cancellationToken);

        var allAnswers = await _db.InspectionAnswers
            .Where(a => a.InspectionId == inspection.Id)
            .ToListAsync(cancellationToken);

        inspection.AnsweredQuestions  = allAnswers.Count;
        inspection.ConformingCount    = allAnswers.Count(a => a.IsConforming == true);
        inspection.NonConformingCount = allAnswers.Count(a => a.IsConforming == false);
        inspection.NaCount            = allAnswers.Count(a => a.IsNa);
        inspection.Latitude           = request.Latitude;
        inspection.Longitude          = request.Longitude;
        inspection.WeatherConditions  = request.WeatherConditions;
        inspection.Notes              = request.Notes;

        // Calcular puntaje si todas las preguntas están respondidas
        if (inspection.AnsweredQuestions >= inspection.TotalQuestions)
        {
            inspection.Status      = InspectionStatus.Finalizada;
            inspection.FinishedAt  = now;
            inspection.Score       = inspection.TotalQuestions > 0
                ? Math.Round((decimal)inspection.ConformingCount / inspection.TotalQuestions * 100, 2)
                : 0;
            inspection.Passed = inspection.PassingScore == null || inspection.Score >= inspection.PassingScore;

            // Auto-crear NC desde preguntas críticas no conformes
            await AutoCreateObservations(inspection, allAnswers, cancellationToken);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return new InspectionDto(
            inspection.Id, inspection.ProjectId, inspection.TemplateId, inspection.Code, inspection.Title,
            inspection.InspectionType, inspection.Status, inspection.Priority,
            inspection.ScheduledDate, inspection.StartedAt, inspection.FinishedAt,
            inspection.AssignedToId, null, inspection.ContractorId, null,
            inspection.StageId, inspection.SectorId, inspection.UnitId,
            inspection.Score, inspection.Passed,
            inspection.TotalQuestions, inspection.AnsweredQuestions,
            inspection.ConformingCount, inspection.NonConformingCount,
            inspection.IsOfflineCreated, inspection.Notes, inspection.CreatedAt);
    }

    private async Task AutoCreateObservations(Inspection inspection, List<InspectionAnswer> answers, CancellationToken ct)
    {
        var criticalFailed = answers.Where(a => a.IsConforming == false).ToList();
        foreach (var answer in criticalFailed)
        {
            var question = await _db.TemplateQuestions.FirstOrDefaultAsync(q => q.Id == answer.QuestionId, ct);
            if (question == null || !question.IsCritical) continue;

            // Solo crear si no existe ya una NC para esta respuesta
            var exists = await _db.Observations.AnyAsync(o => o.AnswerId == answer.Id, ct);
            if (exists) continue;

            var year = DateTime.UtcNow.Year;
            var counter = await _db.SequenceCounters
                .FirstOrDefaultAsync(s => s.EntityType == "observation" && s.Year == year, ct);
            if (counter == null) { counter = new SequenceCounter { TenantId = inspection.TenantId, EntityType = "observation", Prefix = "OBS", Year = year, LastValue = 1 }; _db.SequenceCounters.Add(counter); }
            else counter.LastValue++;

            _db.Observations.Add(new Observation
            {
                TenantId     = inspection.TenantId,
                ProjectId    = inspection.ProjectId,
                InspectionId = inspection.Id,
                AnswerId     = answer.Id,
                Code         = $"OBS-{year}-{counter.LastValue:D4}",
                Title        = $"NC Automática: {question.QuestionText[..Math.Min(100, question.QuestionText.Length)]}",
                Description  = $"Pregunta crítica no conforme en inspección {inspection.Code}.",
                Severity     = ObservationSeverity.Alta,
                DetectedAt   = DateTime.UtcNow,
                DetectedBy   = inspection.CreatedBy,
                CreatedBy    = inspection.CreatedBy
            });
        }
    }
}
