using AgileFlow.Application.Dtos;

namespace AgileFlow.Application.Ports;

/// <summary>
/// Puerto de salida hacia el canal de tiempo real. Application dispara estos
/// eventos tras confirmar cambios en BD; el adaptador concreto
/// decide cómo entregarlos solo a las sesiones suscritas al
/// tablero (grupo = projectId).
/// </summary>
public interface IBoardRealtimeNotifier
{
    Task TaskCreatedAsync(Guid projectId, KanbanTaskDto task, CancellationToken ct = default);
    Task TaskUpdatedAsync(Guid projectId, KanbanTaskDto task, CancellationToken ct = default);
    Task TaskDeletedAsync(Guid projectId, Guid taskId, CancellationToken ct = default);
    Task TaskMovedAsync(Guid projectId, KanbanTaskDto task, CancellationToken ct = default);
}
