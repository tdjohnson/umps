using System.Collections.Concurrent;
using System.Text.Json;
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

    // Returns the session, or null if it is over, closed for joining or full
    public async Task<Session?> JoinSession(string sessionId)
    {
        if (SessionStore.GroupOf(Context.ConnectionId) == SessionStore.GroupName(sessionId))
        {
            return SessionStore.Get(sessionId);
        }

        var moved = await MoveTo(SessionStore.GroupName(sessionId), () => SessionStore.Join(Context.ConnectionId, sessionId));
        return moved ? SessionStore.Get(sessionId) : null;
    }

    public async Task LeaveSession()
    {
        if (SessionStore.GroupOf(Context.ConnectionId) != SessionStore.Lobby)
        {
            await MoveTo(SessionStore.Lobby, () => { SessionStore.Leave(Context.ConnectionId); return true; });
        }
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
            var (name, by) = ParseDefeated(e.destination);
            if (!string.IsNullOrEmpty(name))
            {
                SessionStore.AddDefeat(Context.ConnectionId, name, by);
                ScoreStore.AddDefeat(name);
                await Clients.All.SendAsync("ScoresUpdated", ScoreStore.Scores);
            }
        }
    }

    // destination is either the plain name or JSON {"name": victim, "by": last player who hit}
    private static (string? name, string? by) ParseDefeated(string destination)
    {
        if (destination.TrimStart().StartsWith("{"))
        {
            try
            {
                using var json = JsonDocument.Parse(destination);
                var name = json.RootElement.TryGetProperty("name", out var n) && n.ValueKind == JsonValueKind.String ? n.GetString() : null;
                var by = json.RootElement.TryGetProperty("by", out var b) && b.ValueKind == JsonValueKind.String ? b.GetString() : null;
                return (name, by);
            }
            catch (JsonException)
            {
            }
        }
        return (destination, null);
    }

    // The other players see the move as a player who left
    private async Task<bool> MoveTo(string group, Func<bool> update)
    {
        var current = SessionStore.GroupOf(Context.ConnectionId);
        if (!update())
        {
            return false;
        }

        if (connectionPlayers.TryGetValue(Context.ConnectionId, out var playerId))
        {
            await Clients.GroupExcept(current, Context.ConnectionId).SendAsync("ReceiveEvent", new Event { type = "left", source = playerId, destination = "" });
            Console.WriteLine("event: left " + playerId);
        }
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, current);
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        return true;
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
