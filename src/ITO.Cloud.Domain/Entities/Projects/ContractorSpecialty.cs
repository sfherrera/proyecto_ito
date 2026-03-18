namespace ITO.Cloud.Domain.Entities.Projects;

public class ContractorSpecialty
{
    public Guid ContractorId { get; set; }
    public Guid SpecialtyId { get; set; }

    public Contractor Contractor { get; set; } = null!;
    public Specialty Specialty { get; set; } = null!;
}
