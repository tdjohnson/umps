using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace umps.Hubs;

public class ControlHub : Hub
{
    public static List<string> ConnectedClients = new List<string>();

    // Last player id seen in SendData, per connection
    private static ConcurrentDictionary<string, string> connectionPlayers = new ConcurrentDictionary<string, string>();

    public override async Task OnConnectedAsync()
    {
        lock (ConnectedClients)
        {
            ConnectedClients.Add(Context.ConnectionId);
        }
        await Groups.AddToGroupAsync(Context.ConnectionId, SessionStore.Lobby);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        lock (ConnectedClients)
        {
            ConnectedClients.Remove(Context.ConnectionId);
        }

        // Tell the remaining clients which player left
        await SendLeft();
        connectionPlayers.TryRemove(Context.ConnectionId, out _);
        SessionStore.Leave(Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }

    // Returns the session, or null if it does not exist or is over
    public async Task<Session?> JoinSession(string sessionId)
    {
        var session = SessionStore.Get(sessionId);
        if (session == null)
        {
            return null;
        }

        await MoveTo(SessionStore.GroupName(sessionId), () => SessionStore.Join(Context.ConnectionId, sessionId));
        return SessionStore.Get(sessionId);
    }

    public async Task LeaveSession()
    {
        await MoveTo(SessionStore.Lobby, () => SessionStore.Leave(Context.ConnectionId));
    }

    public async Task SendData(Player player)
    {
        if (!string.IsNullOrEmpty(player.id))
        {
            connectionPlayers[Context.ConnectionId] = player.id;
        }

        // Broadcast the data to all clients in the same session
        await Clients.Group(SessionStore.GroupOf(Context.ConnectionId)).SendAsync("ReceiveData", player);
        Console.WriteLine("player: " + player.id + " " + player.type + " " + player.x + " " + player.y + " " + player.z  + " " + player.xd + " " + player.yd + " " + player.zd);
    }

    public async Task SendEvent(Event e)
    {
        // Broadcast the event to all clients in the same session
        await Clients.Group(SessionStore.GroupOf(Context.ConnectionId)).SendAsync("ReceiveEvent", e);
        Console.WriteLine("event: " + e.type + " " + e.source + " " + e.destination);

        if (e.type == "defeated" && !string.IsNullOrEmpty(e.destination))
        {
            SessionStore.AddDefeat(Context.ConnectionId, e.destination);
            ScoreStore.AddDefeat(e.destination);
            await Clients.All.SendAsync("ScoresUpdated", ScoreStore.Scores);
        }
    }

    private async Task MoveTo(string group, Action update)
    {
        var current = SessionStore.GroupOf(Context.ConnectionId);
        if (current == group)
        {
            return;
        }

        await SendLeft();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, current);
        update();
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
    }

    private async Task SendLeft()
    {
        if (connectionPlayers.TryGetValue(Context.ConnectionId, out var playerId))
        {
            await Clients.GroupExcept(SessionStore.GroupOf(Context.ConnectionId), Context.ConnectionId).SendAsync("ReceiveEvent", new Event { type = "left", source = playerId, destination = "" });
            Console.WriteLine("event: left " + playerId);
        }
    }
}
