using AgileFlow.Application.Ports;
using AgileFlow.Application.UseCases.Columns;
using AgileFlow.Domain.Entities;
using AgileFlow.Domain.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace AgileFlow.Tests.Application;

public class ReorderBoardColumnUseCaseTests
{
    private readonly IBoardColumnRepository _columnRepository = Substitute.For<IBoardColumnRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly ReorderBoardColumnUseCase _sut;

    private readonly Guid _projectId = Guid.NewGuid();

    public ReorderBoardColumnUseCaseTests()
    {
        _sut = new ReorderBoardColumnUseCase(_columnRepository, _unitOfWork);
    }

    [Fact]
    public async Task ExecuteAsync_EntreDosColumnas_CalculaLaPosicionIntermediaYPersiste()
    {
        var column = new BoardColumn("Hecho", 4096m, _projectId);
        var anterior = new BoardColumn("Por hacer", 1024m, _projectId);
        var siguiente = new BoardColumn("En progreso", 2048m, _projectId);

        _columnRepository.GetByIdAsync(column.Id, Arg.Any<CancellationToken>()).Returns(column);
        _columnRepository.GetNeighborsAsync(_projectId, 1, column.Id, Arg.Any<CancellationToken>())
            .Returns((anterior, siguiente));

        var dto = await _sut.ExecuteAsync(new ReorderBoardColumnCommand(column.Id, 1));

        dto.Position.Should().Be(1536m);
        column.Position.Should().Be(1536m);
        await _unitOfWork.Received(1).CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteAsync_UsaElServicioDeDominioCompartidoConLasTareas()
    {
        var previousPosition = 1024m;
        var nextPosition = 2048m;

        var expected = TaskOrderingService.CalculateNewPosition(previousPosition, nextPosition);

        var column = new BoardColumn("Hecho", 4096m, _projectId);
        var anterior = new BoardColumn("Por hacer", previousPosition, _projectId);
        var siguiente = new BoardColumn("En progreso", nextPosition, _projectId);

        _columnRepository.GetByIdAsync(column.Id, Arg.Any<CancellationToken>()).Returns(column);
        _columnRepository.GetNeighborsAsync(_projectId, 1, column.Id, Arg.Any<CancellationToken>())
            .Returns((anterior, siguiente));

        var dto = await _sut.ExecuteAsync(new ReorderBoardColumnCommand(column.Id, 1));

        dto.Position.Should().Be(expected);
    }
}
