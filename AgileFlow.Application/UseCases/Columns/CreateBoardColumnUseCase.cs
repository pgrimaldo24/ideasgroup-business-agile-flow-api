using AgileFlow.Application.Common;
using AgileFlow.Application.Dtos;
using AgileFlow.Application.Ports;
using AgileFlow.Domain.Entities;
using AgileFlow.Domain.Services;

namespace AgileFlow.Application.UseCases.Columns;

public record CreateBoardColumnCommand(Guid ProjectId, string Name);

public class CreateBoardColumnUseCase
{
    private readonly IProjectRepository _projectRepository;
    private readonly IBoardColumnRepository _columnRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateBoardColumnUseCase(
        IProjectRepository projectRepository,
        IBoardColumnRepository columnRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _columnRepository = columnRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<BoardColumnDto> ExecuteAsync(CreateBoardColumnCommand command, CancellationToken ct = default)
    {
        if (!await _projectRepository.ExistsAsync(command.ProjectId, ct))
            throw new NotFoundException(nameof(Project), command.ProjectId);

        var lastPosition = await _columnRepository.GetLastPositionAsync(command.ProjectId, ct);
        var position = TaskOrderingService.CalculateNewPosition(lastPosition, null);

        var column = new BoardColumn(command.Name, position, command.ProjectId);

        await _columnRepository.AddAsync(column, ct);
        await _unitOfWork.CommitAsync(ct);

        return ToDto(column);
    }

    private static BoardColumnDto ToDto(BoardColumn column) => new(
        column.Id, column.Name, column.Position, column.ProjectId, Array.Empty<KanbanTaskDto>());
}
