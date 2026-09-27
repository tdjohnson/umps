using System.Collections.Concurrent;

// Running sessions, who is in them and the defeats counted in each
public static class SessionStore
{
    public const int DurationSeconds = 120;
    public const string Lobby = "lobby";

    private class Entry
    {
        public string id = "";
        public DateTime startedAt;
        public DateTime endsAt;
        public ConcurrentDictionary<string, int> scores = new ConcurrentDictionary<string, int>();
    }

    private static ConcurrentDictionary<string, Entry> sessions = new ConcurrentDictionary<string, Entry>();

    // Session id per connection, connections without an entry are in the lobby
    private static ConcurrentDictionary<string, string> connectionSessions = new ConcurrentDictionary<string, string>();

    public static string GroupName(string sessionId)
    {
        return "session:" + sessionId;
    }

    public static Session Create()
    {
        var now = DateTime.UtcNow;
        var entry = new Entry
        {
            id = Guid.NewGuid().ToString(),
            startedAt = now,
            endsAt = now.AddSeconds(DurationSeconds)
        };
        sessions[entry.id] = entry;
        return ToSession(entry);
    }

    public static List<Session> GetRunning()
    {
        var now = DateTime.UtcNow;
        return sessions.Values.Where(s => s.endsAt > now).OrderBy(s => s.startedAt).Select(ToSession).ToList();
    }

    public static Session? Get(string sessionId)
    {
        if (sessionId != null && sessions.TryGetValue(sessionId, out var entry) && entry.endsAt > DateTime.UtcNow)
        {
            return ToSession(entry);
        }
        return null;
    }

    // Group the connection currently sends to and receives from
    public static string GroupOf(string connectionId)
    {
        if (connectionSessions.TryGetValue(connectionId, out var sessionId))
        {
            return GroupName(sessionId);
        }
        return Lobby;
    }

    public static void Join(string connectionId, string sessionId)
    {
        connectionSessions[connectionId] = sessionId;
    }

    public static void Leave(string connectionId)
    {
        connectionSessions.TryRemove(connectionId, out _);
    }

    public static void AddDefeat(string connectionId, string name)
    {
        if (connectionSessions.TryGetValue(connectionId, out var sessionId) && sessions.TryGetValue(sessionId, out var entry))
        {
            entry.scores.AddOrUpdate(name, 1, (key, count) => count + 1);
        }
    }

    // Removes the sessions whose time is up and returns them with their scores and connections
    public static List<(string id, Dictionary<string, int> scores, List<string> connections)> TakeExpired()
    {
        var expired = new List<(string, Dictionary<string, int>, List<string>)>();
        var now = DateTime.UtcNow;
        foreach (var entry in sessions.Values.Where(s => s.endsAt <= now).ToList())
        {
            if (sessions.TryRemove(entry.id, out _))
            {
                var connections = connectionSessions.Where(c => c.Value == entry.id).Select(c => c.Key).ToList();
                foreach (var connection in connections)
                {
                    connectionSessions.TryRemove(connection, out _);
                }
                expired.Add((entry.id, new Dictionary<string, int>(entry.scores), connections));
            }
        }
        return expired;
    }

    private static Session ToSession(Entry entry)
    {
        return new Session
        {
            id = entry.id,
            startedAt = entry.startedAt,
            endsAt = entry.endsAt,
            secondsRemaining = Math.Max(0, (int)Math.Ceiling((entry.endsAt - DateTime.UtcNow).TotalSeconds)),
            playerCount = connectionSessions.Count(c => c.Value == entry.id)
        };
    }
}
