using ITO.Cloud.Application.Common.Interfaces;
using ITO.Cloud.Domain.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.API.Controllers;

[Authorize]
public class ReportsController : BaseApiController
{
    private readonly IReportService     _reports;
    private readonly IApplicationDbContext _db;

    public ReportsController(IReportService reports, IApplicationDbContext db)
    {
        _reports = reports;
        _db      = db;
    }

    // GET /api/reports/inspections/{id}/pdf
    [HttpGet("inspections/{id:guid}/pdf")]
    public async Task<IActionResult> InspectionPdf(Guid id, CancellationToken ct)
    {
        // Cargar inspección + respuestas + secciones
        var inspection = await _db.Inspections
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id, ct);

        if (inspection == null) return NotFound();

        var answers = await _db.InspectionAnswers
            .AsNoTracking()
            .Where(a => a.InspectionId == id)
            .ToListAsync(ct);

        var sections = await _db.TemplateSections
            .AsNoTracking()
            .Where(s => s.TemplateId == inspection.TemplateId)
            .OrderBy(s => s.OrderIndex)
            .ToListAsync(ct);

        var questions = await _db.TemplateQuestions
            .AsNoTracking()
            .Where(q => q.Section.TemplateId == inspection.TemplateId)
            .ToListAsync(ct);

        // Mapear a ReportData
        var sectionData = sections.Select(s => new SectionReportData(
            s.Title,
            s.OrderIndex,
            questions.Where(q => q.SectionId == s.Id)
                     .OrderBy(q => q.OrderIndex)
                     .Select(q =>
                     {
                         var ans = answers.FirstOrDefault(a => a.QuestionId == q.Id);
                         return new AnswerReportData(
                             q.QuestionText,
                             q.IsCritical,
                             ans?.AnswerValue,
                             ans?.IsConforming,
                             ans?.IsNa ?? false,
                             ans?.Notes
                         );
                     }).ToList()
        )).ToList();

        var data = new InspectionReportData(
            inspection.Code,
            inspection.Title,
            inspection.ProjectId.ToString(),  // se puede enriquecer con join
            inspection.Title,
            null,
            inspection.ScheduledDate,
            inspection.FinishedAt,
            inspection.Status.ToString(),
            inspection.Score,
            inspection.Passed,
            inspection.WeatherConditions,
            inspection.Notes,
            sectionData
        );

        var pdf = _reports.GenerateInspectionPdf(data);
        return File(pdf, "application/pdf", $"inspeccion_{inspection.Code}.pdf");
    }

    // GET /api/reports/projects/{id}/observations/excel
    [HttpGet("projects/{id:guid}/observations/excel")]
    public async Task<IActionResult> ObservationsExcel(Guid id, CancellationToken ct)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (project == null) return NotFound();

        var observations = await _db.Observations
            .AsNoTracking()
            .Where(o => o.ProjectId == id)
            .OrderByDescending(o => o.Severity)
            .ThenByDescending(o => o.DetectedAt)
            .ToListAsync(ct);

        var rows = observations.Select(o => new ObsRow(
            o.Code,
            o.Title,
            o.Severity.ToString(),
            o.Status.ToString(),
            null,  // ContractorName — requiere join adicional
            o.DueDate,
            o.DueDate.HasValue && o.DueDate < DateOnly.FromDateTime(DateTime.UtcNow)
                && o.Status != Domain.Enums.ObservationStatus.Cerrada,
            o.ClosedAt?.ToString("dd/MM/yyyy HH:mm")
        )).ToList();

        var data = new ObservationsReportData(
            project.Code,
            project.Name,
            DateTime.UtcNow,
            rows
        );

        var excel = _reports.GenerateObservationsExcel(data);
        return File(excel,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"observaciones_{project.Code}_{DateTime.Now:yyyyMMdd}.xlsx");
    }
}
