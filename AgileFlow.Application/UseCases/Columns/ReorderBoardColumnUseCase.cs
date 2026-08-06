using AgileFlow.Application.Common;
using AgileFlow.Application.Dtos;
using AgileFlow.Application.Ports;
using AgileFlow.Domain.Entities;
using AgileFlow.Domain.Services;

namespace AgileFlow.Application.UseCases.Columns;

public record ReorderBoardColumnCommand(Guid ColumnId, int TargetIndex);

public class ReorderBoardColumnUseCase
{
    private readonly IBoardColumnRepository _columnRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ReorderBoardColumnUseCase(IBoardColumnRepository columnRepository, IUnitOfWork unitOfWork)
    {
        _columnRepository = columnRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BoardColumnDto> ExecuteAsync(ReorderBoardColumnCommand command, CancellationToken ct = default)
    {
        var column = await _columnRepository.GetByIdAsync(command.ColumnId, ct)
            ?? throw new NotFoundException(nameof(BoardColumn), command.ColumnId);

        var (previous, next) = await _columnRepository.GetNeighborsAsync(
            column.ProjectId, command.TargetIndex, excludeColumnId: column.Id, ct);

        var newPosition = TaskOrderingService.CalculateNewPosition(previous?.Position, next?.Position);

        column.MoveTo(newPosition);

        await _unitOfWork.CommitAsync(ct);

        return ToDto(column);
    }

    private static BoardColumnDto ToDto(BoardColumn column) => new(
        column.Id, column.Name, column.Position, column.ProjectId, Array.Empty<KanbanTaskDto>());
}
