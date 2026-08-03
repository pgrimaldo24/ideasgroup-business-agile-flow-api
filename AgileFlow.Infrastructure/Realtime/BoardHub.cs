using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace AgileFlow.Infrastructure.Realtime;

/// <summary>
/// Hub de SignalR (elegido como tecnología de tiempo real; alternativas
/// descartadas: WebSocket puro exige reimplementar reconexión/heartbeat que
/// SignalR ya resuelve, y SSE es unidireccional server→client, insuficiente
/// si más adelante el cliente necesita invocar métodos del hub — ver README
/// para la justificación completa).
/// </summary>
[Authorize]
public class BoardHub : Hub
{
    private static string GroupName(Guid projectId) => $"board-{projectId}";

    public async Task SubscribeToBoard(Guid projectId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(projectId));
    }

    public async Task UnsubscribeFromBoard(Guid projectId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(projectId));
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
