using Newtonsoft.Json;

namespace HtwDiscordBot.Services;

public class ConfigService
{
    public readonly Config Config;

    public ConfigService(string path = "config.json")
    {
        if (!File.Exists(path))
        {
            File.WriteAllText(path, JsonConvert.SerializeObject(new Config(), formatting: Formatting.Indented));
            Console.WriteLine($"New {path} file generated. Please configure and restart");
            Environment.Exit(-1);
        }

        Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path)) ??
                 throw new Exception("Config is null");
    }

    public void SaveConfig()
    {
        File.WriteAllText("config.json", JsonConvert.SerializeObject(Config, formatting: Formatting.Indented));
    }
}

public class Config
{
    [JsonRequired] public string Token { get; } = "";
    [JsonRequired] public ulong ScoreboardChannelID { get; }
    [JsonRequired] public ulong ChallengeChannelID { get; }
    [JsonRequired] public ulong GermanForumChannelID { get; }
    [JsonRequired] public ulong EnglishForumChannelID { get; }
    [JsonRequired] public ulong RolesChannelID { get; }
    [JsonRequired] public ulong ActiveHackerRoleID { get; }
    [JsonRequired] public ulong CTFRoleID { get; }
    [JsonRequired] public ulong ActiveHackerMessageID { get; set; }
    [JsonRequired] public ulong CTFMessageID { get; set; }
    [JsonRequired] public ulong ServerID { get; }
    [JsonRequired] public string AuthKey { get; } = "";

    public const string Url = "https://hack.arrrg.de/";

    [JsonConstructor]
    public Config(string token, ulong scoreboardChannelID, ulong challengeChannelID, ulong germanForumChannelID,
        ulong englishForumChannelID,
        ulong rolesChannelID, ulong activeHackerRoleID, ulong ctfRoleID, ulong activeHackerMessageID,
        ulong ctfMessageID, ulong serverID, string authKey)
    {
        Token = token;
        ScoreboardChannelID = scoreboardChannelID;
        ChallengeChannelID = challengeChannelID;
        GermanForumChannelID = germanForumChannelID;
        EnglishForumChannelID = englishForumChannelID;
        RolesChannelID = rolesChannelID;
        ServerID = serverID;
        ActiveHackerRoleID = activeHackerRoleID;
        CTFRoleID = ctfRoleID;
        ActiveHackerMessageID = activeHackerMessageID;
        CTFMessageID = ctfMessageID;
        AuthKey = authKey;
    }

    public Config()
    {
    }
}