using System.Collections.Concurrent;

// Running sessions, who is in them and the defeats counted in each
public static class SessionStore
{
    public static int DurationSeconds = 120;
    // Joining closes this many seconds before the end
    public static int JoinClosesSeconds = 15;
    public static int MaxPlayers = 12;
    // The next round starts this many seconds after a session ends
    public static int PauseSeconds = 10;
    public const string Lobby = "lobby";

    private class Entry
    {
        public string id = "";
        public string name = "";
        public DateTime startedAt;
        public DateTime endsAt;
        // False for a follow-up round until its players got 'sessionStarted'
        public bool announced = true;
        public ConcurrentDictionary<string, int> kills = new ConcurrentDictionary<string, int>();
        public ConcurrentDictionary<string, int> defeats = new ConcurrentDictionary<string, int>();
    }

    private static int sessionNumber = 0;

    private static ConcurrentDictionary<string, Entry> sessions = new ConcurrentDictionary<string, Entry>();

    // Session id per connection, connections without an entry are in the lobby
    private static ConcurrentDictionary<string, string> connectionSessions = new ConcurrentDictionary<string, string>();

    public static string GroupName(string sessionId)
    {
        return "session:" + sessionId;
    }

    public static Session Create()
    {
        return ToSession(NewEntry(DateTime.UtcNow, true));
    }

    private static Entry NewEntry(DateTime startsAt, bool announced)
    {
        var entry = new Entry
        {
            id = Guid.NewGuid().ToString(),
            name = "Hofgang " + Interlocked.Increment(ref sessionNumber),
            startedAt = startsAt,
            endsAt = startsAt.AddSeconds(DurationSeconds),
            announced = announced
        };
        sessions[entry.id] = entry;
        return entry;
    }

    // Sessions that can still be joined
    public static List<Session> GetRunning()
    {
        var now = DateTime.UtcNow;
        var closing = now.AddSeconds(JoinClosesSeconds);
        return sessions.Values.Where(s => s.startedAt <= now && s.endsAt > closing).OrderBy(s => s.startedAt).Select(ToSession).ToList();
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

    // False if the session is over, closed for joining or full
    public static bool Join(string connectionId, string sessionId)
    {
        lock (connectionSessions)
        {
            if (sessionId == null || !sessions.TryGetValue(sessionId, out var entry))
            {
                return false;
            }
            if (entry.startedAt > DateTime.UtcNow || entry.endsAt <= DateTime.UtcNow.AddSeconds(JoinClosesSeconds))
            {
                return false;
            }
            if (connectionSessions.Count(c => c.Value == sessionId) >= MaxPlayers)
            {
                return false;
            }
            connectionSessions[connectionId] = sessionId;
            return true;
        }
    }

    public static void Leave(string connectionId)
    {
        connectionSessions.TryRemove(connectionId, out _);
    }

    public static void AddDefeat(string connectionId, string name, string? by)
    {
        if (connectionSessions.TryGetValue(connectionId, out var sessionId) && sessions.TryGetValue(sessionId, out var entry))
        {
            entry.defeats.AddOrUpdate(name, 1, (key, count) => count + 1);
            if (!string.IsNullOrEmpty(by))
            {
                entry.kills.AddOrUpdate(by, 1, (key, count) => count + 1);
            }
        }
    }

    // Removes the sessions whose time is up and moves their players into a follow-up round
    // that starts after the pause. next is null if nobody was left to play on.
    public static List<(string id, string? next, Dictionary<string, int> kills, Dictionary<string, int> defeats, List<string> connections)> TakeExpired()
    {
        var expired = new List<(string, string?, Dictionary<string, int>, Dictionary<string, int>, List<string>)>();
        var now = DateTime.UtcNow;
        lock (connectionSessions)
        {
            foreach (var entry in sessions.Values.Where(s => s.endsAt <= now).ToList())
            {
                if (sessions.TryRemove(entry.id, out _))
                {
                    var connections = connectionSessions.Where(c => c.Value == entry.id).Select(c => c.Key).ToList();
                    string? next = null;
                    if (connections.Count > 0)
                    {
                        next = NewEntry(now.AddSeconds(PauseSeconds), false).id;
                        foreach (var connection in connections)
                        {
                            connectionSessions[connection] = next;
                        }
                    }
                    expired.Add((entry.id, next, new Dictionary<string, int>(entry.kills), new Dictionary<string, int>(entry.defeats), connections));
                }
            }
        }
        return expired;
    }

    // Follow-up rounds whose pause is over. Rounds everybody left during the pause are dropped.
    public static List<Session> TakeStarting()
    {
        var starting = new List<Session>();
        var now = DateTime.UtcNow;
        lock (connectionSessions)
        {
            foreach (var entry in sessions.Values.Where(s => !s.announced && s.startedAt <= now).ToList())
            {
                entry.announced = true;
                var session = ToSession(entry);
                if (session.playerCount == 0)
                {
                    sessions.TryRemove(entry.id, out _);
                }
                else
                {
                    starting.Add(session);
                }
            }
        }
        return starting;
    }

    private static Session ToSession(Entry entry)
    {
        return new Session
        {
            id = entry.id,
            name = entry.name,
            maxPlayers = MaxPlayers,
            startedAt = entry.startedAt,
            endsAt = entry.endsAt,
            secondsRemaining = Math.Max(0, (int)Math.Ceiling((entry.endsAt - DateTime.UtcNow).TotalSeconds)),
            playerCount = connectionSessions.Count(c => c.Value == entry.id)
        };
    }
}
