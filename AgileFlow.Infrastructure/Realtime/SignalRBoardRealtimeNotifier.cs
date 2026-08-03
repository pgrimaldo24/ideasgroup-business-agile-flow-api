using AgileFlow.Application.Dtos;
using AgileFlow.Application.Ports;
using Microsoft.AspNetCore.SignalR;

namespace AgileFlow.Infrastructure.Realtime;

public class SignalRBoardRealtimeNotifier : IBoardRealtimeNotifier
{
    private readonly IHubContext<BoardHub> _hubContext;

    public SignalRBoardRealtimeNotifier(IHubContext<BoardHub> hubContext) => _hubContext = hubContext;

    private static string GroupName(Guid projectId) => $"board-{projectId}";

    public Task TaskCreatedAsync(Guid projectId, KanbanTaskDto task, CancellationToken ct = default) =>
        _hubContext.Clients.Group(GroupName(projectId)).SendAsync("TaskCreated", task, ct);

    public Task TaskUpdatedAsync(Guid projectId, KanbanTaskDto task, CancellationToken ct = default) =>
        _hubContext.Clients.Group(GroupName(projectId)).SendAsync("TaskUpdated", task, ct);

    public Task TaskDeletedAsync(Guid projectId, Guid taskId, CancellationToken ct = default) =>
        _hubContext.Clients.Group(GroupName(projectId)).SendAsync("TaskDeleted", taskId, ct);

    public Task TaskMovedAsync(Guid projectId, KanbanTaskDto task, CancellationToken ct = default) =>
        _hubContext.Clients.Group(GroupName(projectId)).SendAsync("TaskMoved", task, ct);
}
