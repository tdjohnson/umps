using Microsoft.AspNetCore.SignalR;

namespace umps.Hubs;

public class ControlHub : Hub
{
    public static List<string> ConnectedClients = new List<string>();

    public override async Task OnConnectedAsync()
    {
        ConnectedClients.Add(Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        ConnectedClients.Remove(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendData(Player player)
    {
        // Broadcast the data to all clients
        await Clients.All.SendAsync("ReceiveData", player);
        Console.WriteLine("player: " + player.id + " " + player.x + " " + player.y + " " + player.z  + " " + player.xd + " " + player.yd + " " + player.zd);
    }

    public async Task SendEvent(Event e)
    {
        // Broadcast the event to all clients
        await Clients.All.SendAsync("ReceiveEvent", e);
        Console.WriteLine("event: " + e.type + " " + e.source + " " + e.destination);
    }
}