using AgileFlow.Domain.Common;

namespace AgileFlow.Domain.Entities;

/// <summary>
/// Columna configurable del flujo de trabajo de un proyecto (ej. "Por hacer",
/// "En progreso", "Hecho"). El orden se maneja como posición fraccional
/// (decimal) para permitir reordenar sin reescribir todas las filas —
/// misma estrategia usada en KanbanTask.Position
/// </summary>
public class BoardColumn : Entity
{
    public string Name { get; private set; } = default!;
    public decimal Position { get; private set; }
    public Guid ProjectId { get; private set; }

    private BoardColumn() { } // EF Core

    public BoardColumn(string name, decimal position, Guid projectId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre de la columna es obligatorio.");

        if (projectId == Guid.Empty)
            throw new DomainException("La columna debe pertenecer a un proyecto válido.");

        Name = name.Trim();
        Position = position;
        ProjectId = projectId;
    }

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre de la columna es obligatorio.");

        Name = name.Trim();
    }

    public void MoveTo(decimal newPosition)
    {
        Position = newPosition;
    }
}
