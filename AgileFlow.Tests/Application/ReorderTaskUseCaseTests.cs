using AgileFlow.Application.Common;
using AgileFlow.Application.Dtos;
using AgileFlow.Application.Ports;
using AgileFlow.Application.UseCases.Tasks;
using AgileFlow.Domain.Entities;
using AgileFlow.Domain.Enums;
using AgileFlow.Domain.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AgileFlow.Tests.Application;

public class ReorderTaskUseCaseTests
{
    private readonly IKanbanTaskRepository _taskRepository = Substitute.For<IKanbanTaskRepository>();
    private readonly IBoardColumnRepository _columnRepository = Substitute.For<IBoardColumnRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly IBoardRealtimeNotifier _notifier = Substitute.For<IBoardRealtimeNotifier>();
    private readonly ReorderTaskUseCase _sut;

    private readonly Guid _projectId = Guid.NewGuid();
    private readonly BoardColumn _targetColumn;

    public ReorderTaskUseCaseTests()
    {
        _targetColumn = new BoardColumn("En progreso", 1024m, _projectId);
        _sut = new ReorderTaskUseCase(_taskRepository, _columnRepository, _unitOfWork, _notifier);
    }

    [Fact]
    public async Task ExecuteAsync_EntreDosTareas_CalculaLaPosicionIntermediaYPersiste()
    {
        var task = CrearTarea(posicion: 4096m, columnId: Guid.NewGuid());
        var anterior = CrearTarea(posicion: 1024m, columnId: _targetColumn.Id);
        var siguiente = CrearTarea(posicion: 2048m, columnId: _targetColumn.Id);

        Configurar(task, anterior, siguiente);

        var dto = await _sut.ExecuteAsync(new ReorderTaskCommand(task.Id, _targetColumn.Id, 1));

        dto.Position.Should().Be(1536m);
        dto.ColumnId.Should().Be(_targetColumn.Id);
        task.Position.Should().Be(1536m);
        task.ColumnId.Should().Be(_targetColumn.Id);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_HaciaUnaColumnaVacia_UsaLaPosicionInicial()
    {
        var task = CrearTarea(posicion: 4096m, columnId: Guid.NewGuid());

        Configurar(task, previous: null, next: null);

        var dto = await _sut.ExecuteAsync(new ReorderTaskCommand(task.Id, _targetColumn.Id, 0));

        dto.Position.Should().Be(TaskOrderingService.GapStep);
    }

    [Fact]
    public async Task ExecuteAsync_ExcluyeLaTareaMovidaAlPedirLosVecinos()
    {
        var task = CrearTarea(posicion: 4096m, columnId: _targetColumn.Id);

        Configurar(task, previous: null, next: null);

        await _sut.ExecuteAsync(new ReorderTaskCommand(task.Id, _targetColumn.Id, 2));

        await _taskRepository.Received(1).GetNeighborsAsync(
            _targetColumn.Id, 2, task.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_TrasPersistir_NotificaAlProyectoDeLaColumnaDestino()
    {
        var task = CrearTarea(posicion: 4096m, columnId: Guid.NewGuid());

        Configurar(task, previous: null, next: null);

        var dto = await _sut.ExecuteAsync(new ReorderTaskCommand(task.Id, _targetColumn.Id, 0));

        await _notifier.Received(1).TaskMovedAsync(_projectId, dto, Arg.Any<CancellationToken>());

        // El commit debe ocurrir antes de avisar a los clientes conectados.
        Received.InOrder(() =>
        {
            _unitOfWork.CommitAsync(Arg.Any<CancellationToken>());
            _notifier.TaskMovedAsync(_projectId, Arg.Any<KanbanTaskDto>(), Arg.Any<CancellationToken>());
        });
    }

    [Fact]
    public async Task ExecuteAsync_CuandoLaTareaNoExiste_LanzaNotFound()
    {
        _taskRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((KanbanTask?)null);

        var act = () => _sut.ExecuteAsync(new ReorderTaskCommand(Guid.NewGuid(), _targetColumn.Id, 0));

        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CuandoLaColumnaDestinoNoExiste_LanzaNotFound()
    {
        var task = CrearTarea(posicion: 1024m, columnId: Guid.NewGuid());

        _taskRepository.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);
        _columnRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((BoardColumn?)null);

        var act = () => _sut.ExecuteAsync(new ReorderTaskCommand(task.Id, Guid.NewGuid(), 0));

        await act.Should().ThrowAsync<NotFoundException>();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CuandoNoQuedaHuecoEntreVecinos_NoPersisteNiNotifica()
    {
        var task = CrearTarea(posicion: 4096m, columnId: Guid.NewGuid());
        var anterior = CrearTarea(posicion: 1m, columnId: _targetColumn.Id);
        var siguiente = CrearTarea(posicion: 1.0001m, columnId: _targetColumn.Id);

        Configurar(task, anterior, siguiente);

        var act = () => _sut.ExecuteAsync(new ReorderTaskCommand(task.Id, _targetColumn.Id, 1));

        await act.Should().ThrowAsync<AgileFlow.Domain.Common.DomainException>();
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
        await _notifier.DidNotReceive().TaskMovedAsync(
            Arg.Any<Guid>(), Arg.Any<KanbanTaskDto>(), Arg.Any<CancellationToken>());
    }

    private void Configurar(KanbanTask task, KanbanTask? previous, KanbanTask? next)
    {
        _taskRepository.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);
        _columnRepository.GetByIdAsync(_targetColumn.Id, Arg.Any<CancellationToken>()).Returns(_targetColumn);
        _taskRepository.GetNeighborsAsync(
                _targetColumn.Id, Arg.Any<int>(), Arg.Any<Guid?>(), Arg.Any<CancellationToken>())
            .Returns((previous, next));
    }

    private static KanbanTask CrearTarea(decimal posicion, Guid columnId) =>
        new("Tarea", null, TaskPriority.Media, null, columnId, posicion);
}
