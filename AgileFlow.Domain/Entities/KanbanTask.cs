using AgileFlow.Domain.Common;
using AgileFlow.Domain.Enums;

namespace AgileFlow.Domain.Entities;

/// <summary>
/// Tarea dentro de una columna del tablero. Se llama KanbanTask (y no "Task")
/// para no colisionar con System.Threading.Tasks.Task en el resto del código.
/// </summary>
public class KanbanTask : Entity
{
    public string Title { get; private set; } = default!;
    public string? Description { get; private set; }
    public TaskPriority Priority { get; private set; }
    public string? AssigneeName { get; private set; }
    public Guid ColumnId { get; private set; }
    public decimal Position { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    private KanbanTask() { } // EF Core

    public KanbanTask(string title, string? description, TaskPriority priority, string? assigneeName, Guid columnId, decimal position)
    {
        Validate(title, columnId);

        Title = title.Trim();
        Description = description?.Trim();
        Priority = priority;
        AssigneeName = assigneeName?.Trim();
        ColumnId = columnId;
        Position = position;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Update(string title, string? description, TaskPriority priority, string? assigneeName)
    {
        Validate(title, ColumnId);

        Title = title.Trim();
        Description = description?.Trim();
        Priority = priority;
        AssigneeName = assigneeName?.Trim();
    }


    public void MoveTo(Guid columnId, decimal newPosition)
    {
        if (columnId == Guid.Empty)
            throw new DomainException("La columna destino no es válida.");

        ColumnId = columnId;
        Position = newPosition;
    }

    private static void Validate(string title, Guid columnId)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new DomainException("El título de la tarea es obligatorio.");

        if (columnId == Guid.Empty)
            throw new DomainException("La tarea debe pertenecer a una columna válida.");
    }
}
