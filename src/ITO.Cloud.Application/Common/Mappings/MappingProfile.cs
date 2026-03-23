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
        CreateMap<Inspection, InspectionDto>()
            .ForMember(d => d.AssignedToName, o => o.MapFrom(s => s.AssignedTo != null ? s.AssignedTo.FirstName + " " + s.AssignedTo.LastName : null))
            .ForMember(d => d.ContractorName, o => o.MapFrom(s => s.Contractor != null ? s.Contractor.Name : null));
        CreateMap<InspectionAnswer, InspectionAnswerDto>();
        CreateMap<InspectionEvidence, InspectionEvidenceDto>();
        CreateMap<CreateInspectionDto, Inspection>();

        // Observations
        CreateMap<Observation, ObservationDto>()
            .ForMember(d => d.ContractorName, o => o.MapFrom(s => (string?)null))
            .ForMember(d => d.AssignedToName, o => o.MapFrom(s => (string?)null));
        CreateMap<ObservationHistory, ObservationHistoryDto>();
        CreateMap<CreateObservationDto, Observation>();
    }
}
