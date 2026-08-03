namespace AgileFlow.Application.Ports;

/// <summary>
/// Abstrae el "SaveChanges" de EF Core para que Application no dependa de
/// Infrastructure. Cada caso de uso hace sus cambios sobre las entidades /
/// repositorios y confirma con un único CommitAsync (unidad de trabajo).
/// </summary>
public interface IUnitOfWork
{
    Task<int> CommitAsync(CancellationToken ct = default);
}
