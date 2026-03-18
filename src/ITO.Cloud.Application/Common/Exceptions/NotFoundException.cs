namespace ITO.Cloud.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string entity, object key)
        : base($"{entity} con id '{key}' no fue encontrado.") { }

    public NotFoundException(string message) : base(message) { }
}
