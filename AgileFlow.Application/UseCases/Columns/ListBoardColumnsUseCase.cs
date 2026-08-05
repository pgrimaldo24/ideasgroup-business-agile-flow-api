using AgileFlow.Application.Dtos;
using AgileFlow.Application.Ports;
using AgileFlow.Domain.Entities;

namespace AgileFlow.Application.UseCases.Columns;

public class ListBoardColumnsUseCase
{
    private readonly IBoardColumnRepository _columnRepository;

    public ListBoardColumnsUseCase(IBoardColumnRepository columnRepository) => _columnRepository = columnRepository;

    public async Task<IReadOnlyList<BoardColumnDto>> ExecuteAsync(Guid projectId, CancellationToken ct = default)
    {
        var columns = await _columnRepository.GetByProjectIdAsync(projectId, ct);

        return columns.Select(ToDto).ToList();
    }

    private static BoardColumnDto ToDto(BoardColumn column) => new(
        column.Id, column.Name, column.Position, column.ProjectId, Array.Empty<KanbanTaskDto>());
}
