using System.Collections.Concurrent;
using System.Text.Json;

// Number of "defeated" events per player name, saved to a JSON file on each change
public static class ScoreStore
{
    private static readonly object fileLock = new object();
    private static string filePath = "scores.json";

    public static ConcurrentDictionary<string, int> Scores = new ConcurrentDictionary<string, int>();

    public static void Load(string path)
    {
        filePath = path;
        try
        {
            if (File.Exists(filePath))
            {
                var saved = JsonSerializer.Deserialize<Dictionary<string, int>>(File.ReadAllText(filePath));
                if (saved != null)
                {
                    Scores = new ConcurrentDictionary<string, int>(saved);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("scores: could not load " + filePath + ": " + ex.Message);
        }
    }

    public static void AddDefeat(string name)
    {
        Scores.AddOrUpdate(name, 1, (key, count) => count + 1);
        Save();
    }

    private static void Save()
    {
        try
        {
            lock (fileLock)
            {
                // Write to a temp file first so a crash cannot leave a half-written file
                var tmp = filePath + ".tmp";
                File.WriteAllText(tmp, JsonSerializer.Serialize(Scores));
                File.Move(tmp, filePath, true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("scores: could not save " + filePath + ": " + ex.Message);
        }
    }
}
