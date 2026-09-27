using System.Text.Json;
using Microsoft.AspNetCore.SignalR;

namespace umps.Hubs;

// Ends sessions when their time is up and starts the follow-up rounds
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
                    await hub.Clients.Group(group).SendAsync("ReceiveEvent", new Event { type = "sessionEnded", source = session.id, destination = JsonSerializer.Serialize(new { kills = session.kills, defeats = session.defeats, next = session.next }) });
                    Console.WriteLine("event: sessionEnded " + session.id);

                    // Players of a finished session wait in the follow-up round
                    foreach (var connection in session.connections)
                    {
                        await hub.Groups.RemoveFromGroupAsync(connection, group);
                        await hub.Groups.AddToGroupAsync(connection, SessionStore.GroupName(session.next!));
                    }
                }

                foreach (var session in SessionStore.TakeStarting())
                {
                    await hub.Clients.Group(SessionStore.GroupName(session.id)).SendAsync("ReceiveEvent", new Event { type = "sessionStarted", source = session.id, destination = JsonSerializer.Serialize(session) });
                    Console.WriteLine("event: sessionStarted " + session.id);
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
