namespace ITO.Cloud.API;

/// <summary>Constantes de roles del sistema ITO Cloud.</summary>
public static class Roles
{
    public const string SuperAdmin  = "SuperAdmin";
    public const string AdminTenant = "AdminTenant";
    public const string Director    = "Director";
    public const string Supervisor  = "Supervisor";
    public const string Inspector   = "Inspector";
    public const string Contratista = "Contratista";

    public const string Admins     = "SuperAdmin,AdminTenant";
    public const string Management = "SuperAdmin,AdminTenant,Director";
    public const string Operations = "SuperAdmin,AdminTenant,Director,Supervisor";
    public const string Inspectors = "SuperAdmin,AdminTenant,Director,Supervisor,Inspector";
}
