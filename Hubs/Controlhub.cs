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
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        lock (ConnectedClients)
        {
            ConnectedClients.Remove(Context.ConnectionId);
        }

        // Tell the remaining clients which player left
        if (connectionPlayers.TryRemove(Context.ConnectionId, out var playerId))
        {
            await Clients.Others.SendAsync("ReceiveEvent", new Event { type = "left", source = playerId, destination = "" });
            Console.WriteLine("event: left " + playerId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendData(Player player)
    {
        if (!string.IsNullOrEmpty(player.id))
        {
            connectionPlayers[Context.ConnectionId] = player.id;
        }

        // Broadcast the data to all clients
        await Clients.All.SendAsync("ReceiveData", player);
        Console.WriteLine("player: " + player.id + " " + player.type + " " + player.x + " " + player.y + " " + player.z  + " " + player.xd + " " + player.yd + " " + player.zd);
    }

    public async Task SendEvent(Event e)
    {
        // Broadcast the event to all clients
        await Clients.All.SendAsync("ReceiveEvent", e);
        Console.WriteLine("event: " + e.type + " " + e.source + " " + e.destination);

        if (e.type == "defeated" && !string.IsNullOrEmpty(e.destination))
        {
            ScoreStore.AddDefeat(e.destination);
            await Clients.All.SendAsync("ScoresUpdated", ScoreStore.Scores);
        }
    }
}
