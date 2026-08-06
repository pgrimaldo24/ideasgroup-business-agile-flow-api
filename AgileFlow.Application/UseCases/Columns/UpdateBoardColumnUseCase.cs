using AgileFlow.Application.Common;
using AgileFlow.Application.Dtos;
using AgileFlow.Application.Ports;
using AgileFlow.Domain.Entities;

namespace AgileFlow.Application.UseCases.Columns;

public record UpdateBoardColumnCommand(Guid ColumnId, string Name);

public class UpdateBoardColumnUseCase
{
    private readonly IBoardColumnRepository _columnRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateBoardColumnUseCase(IBoardColumnRepository columnRepository, IUnitOfWork unitOfWork)
    {
        _columnRepository = columnRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BoardColumnDto> ExecuteAsync(UpdateBoardColumnCommand command, CancellationToken ct = default)
    {
        var column = await _columnRepository.GetByIdAsync(command.ColumnId, ct)
            ?? throw new NotFoundException(nameof(BoardColumn), command.ColumnId);

        column.Rename(command.Name);

        await _unitOfWork.CommitAsync(ct);

        return ToDto(column);
    }

    private static BoardColumnDto ToDto(BoardColumn column) => new(
        column.Id, column.Name, column.Position, column.ProjectId, Array.Empty<KanbanTaskDto>());
}
