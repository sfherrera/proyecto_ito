using ITO.Cloud.Application.Common.Interfaces;
using ITO.Cloud.Domain.Entities.Documents;
using ITO.Cloud.Domain.Entities.Identity;
using ITO.Cloud.Domain.Entities.Inspections;
using ITO.Cloud.Domain.Entities.Observations;
using ITO.Cloud.Domain.Entities.Projects;
using ITO.Cloud.Domain.Entities.Templates;
using ITO.Cloud.Domain.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ITO.Cloud.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>, IApplicationDbContext
{
    private readonly ITenantContext _tenantContext;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
    }

    // ── Identity ──────────────────────────────────
    public DbSet<Tenant> Tenants => Set<Tenant>();

    // ── Projects ──────────────────────────────────
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectStage> ProjectStages => Set<ProjectStage>();
    public DbSet<ProjectSector> ProjectSectors => Set<ProjectSector>();
    public DbSet<ProjectUnit> ProjectUnits => Set<ProjectUnit>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<Contractor> Contractors => Set<Contractor>();
    public DbSet<ContractorSpecialty> ContractorSpecialties => Set<ContractorSpecialty>();
    public DbSet<ProjectContractor> ProjectContractors => Set<ProjectContractor>();

    // ── Templates ─────────────────────────────────
    public DbSet<InspectionTemplate> InspectionTemplates => Set<InspectionTemplate>();
    public DbSet<TemplateVersion> TemplateVersions => Set<TemplateVersion>();
    public DbSet<TemplateSection> TemplateSections => Set<TemplateSection>();
    public DbSet<TemplateQuestion> TemplateQuestions => Set<TemplateQuestion>();
    public DbSet<TemplateQuestionOption> TemplateQuestionOptions => Set<TemplateQuestionOption>();

    // ── Inspections ───────────────────────────────
    public DbSet<Inspection> Inspections => Set<Inspection>();
    public DbSet<InspectionAnswer> InspectionAnswers => Set<InspectionAnswer>();
    public DbSet<InspectionEvidence> InspectionEvidence => Set<InspectionEvidence>();

    // ── Observations ──────────────────────────────
    public DbSet<Observation> Observations => Set<Observation>();
    public DbSet<ObservationHistory> ObservationHistory => Set<ObservationHistory>();
    public DbSet<Reinspection> Reinspections => Set<Reinspection>();

    // ── Documents ─────────────────────────────────
    public DbSet<ProjectDocument> ProjectDocuments => Set<ProjectDocument>();

    // ── Transversal ───────────────────────────────
    public DbSet<SequenceCounter> SequenceCounters => Set<SequenceCounter>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Aplicar todas las configuraciones de entidades
        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Renombrar tablas de Identity a snake_case
        builder.Entity<ApplicationUser>().ToTable("users");
        builder.Entity<IdentityRole<Guid>>().ToTable("roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens");

        // Global Query Filters — multi-tenant automático
        ApplyTenantFilters(builder);

        // Soft delete global
        ApplySoftDeleteFilters(builder);
    }

    private void ApplyTenantFilters(ModelBuilder builder)
    {
        // IMPORTANTE: se usa _tenantContext (referencia) para evaluación dinámica
        // en cada query — NO capturar el valor aquí (se evaluaría solo una vez).

        // Companies
        builder.Entity<Company>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId && !e.IsDeleted);

        // Projects
        builder.Entity<Project>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId && !e.IsDeleted);

        builder.Entity<ProjectStage>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId && !e.IsDeleted);

        builder.Entity<ProjectSector>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId && !e.IsDeleted);

        builder.Entity<ProjectUnit>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId && !e.IsDeleted);

        builder.Entity<Specialty>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId && !e.IsDeleted);

        builder.Entity<Contractor>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId && !e.IsDeleted);

        // Templates
        builder.Entity<InspectionTemplate>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId && !e.IsDeleted);

        builder.Entity<TemplateSection>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        builder.Entity<TemplateQuestion>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        // Inspections
        builder.Entity<Inspection>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId && !e.IsDeleted);

        builder.Entity<InspectionAnswer>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        builder.Entity<InspectionEvidence>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        // Observations
        builder.Entity<Observation>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId && !e.IsDeleted);

        builder.Entity<ObservationHistory>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);

        builder.Entity<Reinspection>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId && !e.IsDeleted);

        // Documents
        builder.Entity<ProjectDocument>()
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId && !e.IsDeleted);

        // Users — Identity no acepta filtro con IsAuthenticated; solo filtrar por tenant
        // si hay tenant activo para no bloquear endpoints de auth/seed
        builder.Entity<ApplicationUser>()
            .HasQueryFilter(e => (_tenantContext.TenantId == Guid.Empty || e.TenantId == _tenantContext.TenantId)
                                 && !e.IsDeleted);
    }

    private static void ApplySoftDeleteFilters(ModelBuilder builder)
    {
        // Los filtros de soft delete ya están incluidos en ApplyTenantFilters
        // Este método existe para extensión futura si se necesita filtrar
        // entidades sin tenant (ej: Tenants, Roles del sistema)
    }
}
