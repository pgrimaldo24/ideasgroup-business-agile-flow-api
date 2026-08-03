namespace AgileFlow.Application.Common;

/// <summary>
/// Errores esperados de la capa de aplicación (credenciales inválidas, recurso no encontrado, etc.).
/// </summary>
public class AppException : Exception
{
    public AppException(string message) : base(message) { }
}

public class NotFoundException : AppException
{
    public NotFoundException(string entity, object key)
        : base($"{entity} con identificador '{key}' no fue encontrado.") { }
}

public class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message = "Credenciales inválidas.") : base(message) { }
}
