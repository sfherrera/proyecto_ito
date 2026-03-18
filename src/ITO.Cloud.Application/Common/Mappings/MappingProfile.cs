using AutoMapper;
using ITO.Cloud.Domain.Entities.Identity;
using ITO.Cloud.Domain.Entities.Projects;
using ITO.Cloud.Domain.Entities.Templates;
using ITO.Cloud.Domain.Entities.Inspections;
using ITO.Cloud.Domain.Entities.Observations;
using ITO.Cloud.Application.Features.Companies.DTOs;
using ITO.Cloud.Application.Features.Projects.DTOs;
using ITO.Cloud.Application.Features.Users.DTOs;
using ITO.Cloud.Application.Features.Templates.DTOs;
using ITO.Cloud.Application.Features.Inspections.DTOs;
using ITO.Cloud.Application.Features.Observations.DTOs;

namespace ITO.Cloud.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Identity
        CreateMap<Tenant, TenantDto>();
        CreateMap<ApplicationUser, UserDto>()
            .ForMember(d => d.FullName, o => o.MapFrom(s => s.FullName))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Email));

        // Projects
        CreateMap<Company, CompanyDto>();
        CreateMap<CreateCompanyDto, Company>();
        CreateMap<UpdateCompanyDto, Company>();

        CreateMap<Project, ProjectDto>()
            .ForMember(d => d.CompanyName, o => o.MapFrom(s => s.Company != null ? s.Company.Name : null));
        CreateMap<CreateProjectDto, Project>();
        CreateMap<UpdateProjectDto, Project>();

        CreateMap<ProjectStage, ProjectStageDto>();
        CreateMap<ProjectSector, ProjectSectorDto>();
        CreateMap<ProjectUnit, ProjectUnitDto>();
        CreateMap<Specialty, SpecialtyDto>();
        CreateMap<Contractor, ContractorDto>();

        // Templates
        CreateMap<InspectionTemplate, TemplateDto>();
        CreateMap<TemplateSection, TemplateSectionDto>();
        CreateMap<TemplateQuestion, TemplateQuestionDto>();
        CreateMap<TemplateQuestionOption, TemplateQuestionOptionDto>();
        CreateMap<CreateTemplateDto, InspectionTemplate>();
        CreateMap<CreateTemplateSectionDto, TemplateSection>();
        CreateMap<CreateTemplateQuestionDto, TemplateQuestion>();

        // Inspections
        CreateMap<Inspection, InspectionDto>();
        CreateMap<InspectionAnswer, InspectionAnswerDto>();
        CreateMap<InspectionEvidence, InspectionEvidenceDto>();
        CreateMap<CreateInspectionDto, Inspection>();

        // Observations
        CreateMap<Observation, ObservationDto>();
        CreateMap<ObservationHistory, ObservationHistoryDto>();
        CreateMap<CreateObservationDto, Observation>();
    }
}
