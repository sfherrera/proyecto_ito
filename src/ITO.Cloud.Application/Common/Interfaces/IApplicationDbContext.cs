using ITO.Cloud.Domain.Entities.Documents;
using ITO.Cloud.Domain.Entities.Identity;
using ITO.Cloud.Domain.Entities.Inspections;
using ITO.Cloud.Domain.Entities.Observations;
using ITO.Cloud.Domain.Entities.Projects;
using ITO.Cloud.Domain.Entities.Templates;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.Application.Common.Interfaces;

/// <summary>
/// Contrato del DbContext expuesto hacia la capa Application.
/// Infrastructure implementa esta interfaz en ApplicationDbContext.
/// </summary>
public interface IApplicationDbContext
{
    // Identity
    DbSet<Tenant> Tenants { get; }

    // Projects
    DbSet<Company> Companies { get; }
    DbSet<Project> Projects { get; }
    DbSet<ProjectStage> ProjectStages { get; }
    DbSet<ProjectSector> ProjectSectors { get; }
    DbSet<ProjectUnit> ProjectUnits { get; }
    DbSet<ProjectMember> ProjectMembers { get; }
    DbSet<Specialty> Specialties { get; }
    DbSet<Contractor> Contractors { get; }
    DbSet<ContractorSpecialty> ContractorSpecialties { get; }
    DbSet<ProjectContractor> ProjectContractors { get; }

    // Templates
    DbSet<InspectionTemplate> InspectionTemplates { get; }
    DbSet<TemplateVersion> TemplateVersions { get; }
    DbSet<TemplateSection> TemplateSections { get; }
    DbSet<TemplateQuestion> TemplateQuestions { get; }
    DbSet<TemplateQuestionOption> TemplateQuestionOptions { get; }

    // Inspections
    DbSet<Inspection> Inspections { get; }
    DbSet<InspectionAnswer> InspectionAnswers { get; }
    DbSet<InspectionEvidence> InspectionEvidence { get; }

    // Observations
    DbSet<Observation> Observations { get; }
    DbSet<ObservationHistory> ObservationHistory { get; }
    DbSet<Reinspection> Reinspections { get; }

    // Documents
    DbSet<ProjectDocument> ProjectDocuments { get; }

    // Transversal
    DbSet<SequenceCounter> SequenceCounters { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
