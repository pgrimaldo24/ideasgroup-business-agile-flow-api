using AgileFlow.Application.Common;
using AgileFlow.Application.Ports;
using AgileFlow.Domain.Common;
using AgileFlow.Domain.Entities;

namespace AgileFlow.Application.UseCases.Columns;

public record DeleteBoardColumnCommand(Guid ColumnId);

public class DeleteBoardColumnUseCase
{
    private readonly IBoardColumnRepository _columnRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteBoardColumnUseCase(IBoardColumnRepository columnRepository, IUnitOfWork unitOfWork)
    {
        _columnRepository = columnRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task ExecuteAsync(DeleteBoardColumnCommand command, CancellationToken ct = default)
    {
        var column = await _columnRepository.GetByIdAsync(command.ColumnId, ct)
            ?? throw new NotFoundException(nameof(BoardColumn), command.ColumnId);

        if (await _columnRepository.HasTasksAsync(column.Id, ct))
            throw new DomainException("No se puede eliminar una columna que contiene tareas.");

        _columnRepository.Remove(column);

        await _unitOfWork.CommitAsync(ct);
    }
}
