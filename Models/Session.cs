public class Session
{
    public string id { get; set; } = "";
    public string name { get; set; } = "";
    public DateTime startedAt { get; set; }
    public DateTime endsAt { get; set; }
    public int secondsRemaining { get; set; }
    public int playerCount { get; set; }
    public int maxPlayers { get; set; }
}
