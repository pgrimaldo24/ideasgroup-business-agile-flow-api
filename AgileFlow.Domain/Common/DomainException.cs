namespace AgileFlow.Domain.Common;

/// <summary>
/// Excepción lanzada cuando se viola una regla de negocio del dominio.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message) { }
}
