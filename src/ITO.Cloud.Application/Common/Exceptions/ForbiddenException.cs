namespace ITO.Cloud.Application.Common.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException() : base("No tiene permisos para realizar esta acción.") { }
    public ForbiddenException(string message) : base(message) { }
}

public class ConflictException : Exception
{
    public ConflictException(string message) : base(message) { }
}
