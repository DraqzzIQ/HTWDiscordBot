namespace HTWDiscordBot.Models;
public class ScoreboardEntryModel
{
    public string Name { get; set; }
    public int Score { get; set; }
    public DateTime UpdatedAt { get; set; }
    public int Rank { get; set; }
}