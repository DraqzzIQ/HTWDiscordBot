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
    }

    public class Config
    {
        [JsonRequired]
        public string Username { get; }
        [JsonRequired]
        public string Password { get; }
        [JsonRequired]
        public string Token { get; }
        [JsonRequired]
        public ulong ScoreboardChannelID { get; }
        [JsonRequired]
        public ulong ChallengeChannelID { get; }
        [JsonRequired]
        public ulong ServerID { get; }

        public const string Url = "https://hack.arrrg.de/";

        [JsonConstructor]
        public Config(string username, string password, string token, ulong scoreboardChannelID, ulong challengeChannelID, ulong serverID)
        {
            Username = username;
            Password = password;
            Token = token;
            ScoreboardChannelID = scoreboardChannelID;
            ChallengeChannelID = challengeChannelID;
            ServerID = serverID;
        }
        public Config() { }
    }
}
