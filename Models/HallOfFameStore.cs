using System.Text.Json;

public class HallOfFameEntry
{
    public string name { get; set; } = "";
    public int kills { get; set; }
    public int deaths { get; set; }
    public string date { get; set; } = "";
    public string session { get; set; } = "";
}

// Best single-round results, ranked by kills / max(1, deaths), saved to a JSON file
public static class HallOfFameStore
{
    public const int MaxEntries = 50;

    private static readonly object entriesLock = new object();
    private static string filePath = "halloffame.json";
    private static List<HallOfFameEntry> entries = new List<HallOfFameEntry>();

    public static void Load(string path)
    {
        filePath = path;
        try
        {
            if (File.Exists(filePath))
            {
                var saved = JsonSerializer.Deserialize<List<HallOfFameEntry>>(File.ReadAllText(filePath));
                if (saved != null)
                {
                    entries = Rank(saved);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("hall of fame: could not load " + filePath + ": " + ex.Message);
        }
    }

    public static List<HallOfFameEntry> GetEntries()
    {
        lock (entriesLock)
        {
            return entries.ToList();
        }
    }

    // Adds every player of a finished round with at least one kill
    public static void AddRound(string session, Dictionary<string, int> kills, Dictionary<string, int> defeats)
    {
        var date = DateTime.UtcNow.ToString("o");
        var added = kills.Where(k => k.Value > 0).Select(k => new HallOfFameEntry
        {
            name = k.Key,
            kills = k.Value,
            deaths = defeats.TryGetValue(k.Key, out var d) ? d : 0,
            date = date,
            session = session
        }).ToList();

        if (added.Count == 0)
        {
            return;
        }

        lock (entriesLock)
        {
            entries = Rank(entries.Concat(added));
            Save();
        }
    }

    // Best ratio first, then more kills, then the older result; only the best MaxEntries are kept
    private static List<HallOfFameEntry> Rank(IEnumerable<HallOfFameEntry> list)
    {
        return list
            .OrderByDescending(e => (double)e.kills / Math.Max(1, e.deaths))
            .ThenByDescending(e => e.kills)
            .ThenBy(e => e.date, StringComparer.Ordinal)
            .Take(MaxEntries)
            .ToList();
    }

    private static void Save()
    {
        try
        {
            // Write to a temp file first so a crash cannot leave a half-written file
            var tmp = filePath + ".tmp";
            File.WriteAllText(tmp, JsonSerializer.Serialize(entries));
            File.Move(tmp, filePath, true);
        }
        catch (Exception ex)
        {
            Console.WriteLine("hall of fame: could not save " + filePath + ": " + ex.Message);
        }
    }
}
