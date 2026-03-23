namespace ITO.Cloud.Web.Auth;

/// <summary>Constantes de roles del sistema ITO Cloud.</summary>
public static class Roles
{
    public const string SuperAdmin   = "SuperAdmin";
    public const string AdminTenant  = "AdminTenant";
    public const string Director     = "Director";
    public const string Supervisor   = "Supervisor";
    public const string Inspector    = "Inspector";
    public const string Contratista  = "Contratista";

    // Grupos de roles para usar en [Authorize(Roles = Roles.Admins)]
    public const string Admins      = "SuperAdmin,AdminTenant";
    public const string Management  = "SuperAdmin,AdminTenant,Director";
    public const string Operations  = "SuperAdmin,AdminTenant,Director,Supervisor";
    public const string Inspectors  = "SuperAdmin,AdminTenant,Director,Supervisor,Inspector";
    public const string All         = "SuperAdmin,AdminTenant,Director,Supervisor,Inspector,Contratista";
}
