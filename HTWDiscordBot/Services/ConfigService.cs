using Newtonsoft.Json;

namespace HTWDiscordBot.Services
{
    //Stellt Konfigurationsdaten bereit
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

            Config = JsonConvert.DeserializeObject<Config>(File.ReadAllText(path));
        }

        public void SaveConfig()
        {
            File.WriteAllText("config.json", JsonConvert.SerializeObject(Config, formatting: Formatting.Indented));
        }
    }

    public class Config
    {
        [JsonRequired]
        public string Token { get; }
        [JsonRequired]
        public ulong ScoreboardChannelID { get; }
        [JsonRequired]
        public ulong ChallengeChannelID { get; }
        [JsonRequired]
        public ulong RolesChannelID { get; }
        [JsonRequired]
        public ulong ActiveHackerRoleID { get; }
        [JsonRequired]
        public ulong CTFRoleID { get; }
        [JsonRequired]
        public ulong ActiveHackerMessageID { get; set; }
        [JsonRequired]
        public ulong CTFMessageID { get; set; }
        [JsonRequired]
        public ulong ServerID { get; }

        public const string Url = "https://hack.arrrg.de/";

        [JsonConstructor]
        public Config(string token, ulong scoreboardChannelID, ulong challengeChannelID, ulong rolesChannelID, ulong activeHackerRoleID, ulong ctfRoleID, ulong activeHackerMessageID, ulong ctfMessageID, ulong serverID)
        {
            Token = token;
            ScoreboardChannelID = scoreboardChannelID;
            ChallengeChannelID = challengeChannelID;
            RolesChannelID = rolesChannelID;
            ServerID = serverID;
            ActiveHackerRoleID = activeHackerRoleID;
            CTFRoleID = ctfRoleID;
            ActiveHackerMessageID = activeHackerMessageID;
            CTFMessageID = ctfMessageID;
        }
        public Config() { }
    }
}
