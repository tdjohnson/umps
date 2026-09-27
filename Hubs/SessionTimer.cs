using System.Text.Json;
using Microsoft.AspNetCore.SignalR;

namespace umps.Hubs;

// Ends sessions when their time is up
public class SessionTimer : BackgroundService
{
    private readonly IHubContext<ControlHub> hub;

    public SessionTimer(IHubContext<ControlHub> hub)
    {
        this.hub = hub;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                foreach (var session in SessionStore.TakeExpired())
                {
                    var group = SessionStore.GroupName(session.id);
                    await hub.Clients.Group(group).SendAsync("ReceiveEvent", new Event { type = "sessionEnded", source = session.id, destination = JsonSerializer.Serialize(session.scores) });
                    Console.WriteLine("event: sessionEnded " + session.id);

                    // Players of a finished session go back to the lobby
                    foreach (var connection in session.connections)
                    {
                        await hub.Groups.RemoveFromGroupAsync(connection, group);
                        await hub.Groups.AddToGroupAsync(connection, SessionStore.Lobby);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("sessions: " + ex.Message);
            }

            try
            {
                await Task.Delay(250, stoppingToken);
            }
            catch (TaskCanceledException)
            {
            }
        }
    }
}
