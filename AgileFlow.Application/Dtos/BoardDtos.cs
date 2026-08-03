namespace AgileFlow.Application.Dtos;

public record KanbanTaskDto(
    Guid Id,
    string Title,
    string? Description,
    string Priority,
    string? AssigneeName,
    Guid ColumnId,
    decimal Position,
    DateTime CreatedAtUtc);

public record BoardColumnDto(
    Guid Id,
    string Name,
    decimal Position,
    Guid ProjectId,
    IReadOnlyList<KanbanTaskDto> Tasks);

public record ProjectDto(
    Guid Id,
    string Name,
    string? Description,
    DateTime StartDate,
    DateTime ExpectedEndDate,
    string Status);

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
