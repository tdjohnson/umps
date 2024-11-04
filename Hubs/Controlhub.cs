using Microsoft.AspNetCore.SignalR;

namespace umps.Hubs;

public class ControlHub : Hub
{
    public async Task SendData(Position data)
    {
        // Broadcast the data to all clients
        await Clients.All.SendAsync("ReceiveData", data);
        Console.WriteLine("Received player data: " + data.x + " " + data.y + " " + data.z);
    }
}