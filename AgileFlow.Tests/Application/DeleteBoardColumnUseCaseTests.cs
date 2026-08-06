using AgileFlow.Application.Ports;
using AgileFlow.Application.UseCases.Columns;
using AgileFlow.Domain.Common;
using AgileFlow.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AgileFlow.Tests.Application;

public class DeleteBoardColumnUseCaseTests
{
    private readonly IBoardColumnRepository _columnRepository = Substitute.For<IBoardColumnRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly DeleteBoardColumnUseCase _sut;

    public DeleteBoardColumnUseCaseTests()
    {
        _sut = new DeleteBoardColumnUseCase(_columnRepository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_CuandoLaColumnaTieneTareas_LanzaDomainExceptionYNoPersiste()
    {
        var column = new BoardColumn("En progreso", 1024m, Guid.NewGuid());

        _columnRepository.GetByIdAsync(column.Id, Arg.Any<CancellationToken>()).Returns(column);
        _columnRepository.HasTasksAsync(column.Id, Arg.Any<CancellationToken>()).Returns(true);

        var act = () => _sut.ExecuteAsync(new DeleteBoardColumnCommand(column.Id));

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*No se puede eliminar*tareas*");
        _columnRepository.DidNotReceive().Remove(Arg.Any<BoardColumn>());
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_CuandoLaColumnaNoTieneTareas_LaEliminaYPersiste()
    {
        var column = new BoardColumn("En progreso", 1024m, Guid.NewGuid());

        _columnRepository.GetByIdAsync(column.Id, Arg.Any<CancellationToken>()).Returns(column);
        _columnRepository.HasTasksAsync(column.Id, Arg.Any<CancellationToken>()).Returns(false);

        await _sut.ExecuteAsync(new DeleteBoardColumnCommand(column.Id));

        _columnRepository.Received(1).Remove(column);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }
}
