using Microsoft.AspNetCore.SignalR;

namespace umps.Hubs;

public class ControlHub : Hub
{
    public async Task SendData(Player player)
    {
        // Broadcast the data to all clients
        await Clients.All.SendAsync("ReceiveData", player);
        Console.WriteLine("player: " + player.id + " " + player.x + " " + player.y + " " + player.z  + " " + player.a);
    }
}