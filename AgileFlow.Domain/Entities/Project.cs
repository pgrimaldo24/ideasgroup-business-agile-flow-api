using AgileFlow.Domain.Common;
using AgileFlow.Domain.Enums;

namespace AgileFlow.Domain.Entities;

public class Project : Entity
{
    private readonly List<BoardColumn> _columns = new();

    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime ExpectedEndDate { get; private set; }
    public ProjectStatus Status { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<BoardColumn> Columns => _columns.AsReadOnly();

    private Project() { } // EF Core

    public Project(string name, string? description, DateTime startDate, DateTime expectedEndDate, ProjectStatus status)
    {
        Validate(name, startDate, expectedEndDate);

        Name = name.Trim();
        Description = description?.Trim();
        StartDate = startDate;
        ExpectedEndDate = expectedEndDate;
        Status = status;
        CreatedAtUtc = DateTime.UtcNow;
    }

    public void Update(string name, string? description, DateTime startDate, DateTime expectedEndDate, ProjectStatus status)
    {
        Validate(name, startDate, expectedEndDate);

        Name = name.Trim();
        Description = description?.Trim();
        StartDate = startDate;
        ExpectedEndDate = expectedEndDate;
        Status = status;
    }

    private static void Validate(string name, DateTime startDate, DateTime expectedEndDate)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre del proyecto es obligatorio.");

        if (expectedEndDate < startDate)
            throw new DomainException("La fecha de fin prevista no puede ser anterior a la fecha de inicio.");
    }
}
