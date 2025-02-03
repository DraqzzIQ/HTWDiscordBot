using Newtonsoft.Json;

namespace HtwDiscordBot.Models;

public class ChallengeModel
{
    public int ID { get; set; }

    [JsonIgnore]
    public string URL
    {
        get { return $"https://hack.arrrg.de/challenge/{ID}"; }
    }
}