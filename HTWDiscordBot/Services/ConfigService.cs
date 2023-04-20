using Discord;

namespace HTWDiscordBot.Services
{
    //Stellt Konfigurationsdaten bereit
    public class ConfigService
    {
        public readonly Config Config;

        public ConfigService(LoggingService loggingService, string path = ".env")
        {
            if (!File.Exists(path))
            {
                File.Create(path);
                Console.WriteLine($"New {path} file generated. Please configure and restart");
                Environment.Exit(-1);
            }
            //1. Zeile Username
            //2. Zeile Password
            //3. Zeile Discord Bot Token
            //4. Zeile Scoreboard Channel ID
            //5. Zeile Challenge Channel ID
            //6. Zeile Server ID
            string[] content = File.ReadAllLines(path);

            if (content.Length < 6)
            {
                loggingService.Log(new(LogSeverity.Error, "Config", "Kofigurationsdatei nicht vollständig"));
                Environment.Exit(-1);
            }

            Config = new(content[0].Trim(), content[1].Trim(), content[2].Trim(), ulong.Parse(content[3].Trim()), ulong.Parse(content[4].Trim()), ulong.Parse(content[5].Trim()));
        }
    }

    public class Config
    {
        public readonly string Username;
        public readonly string Password;
        public readonly string Token;
        public readonly ulong ScoreboardChannelID;
        public readonly ulong ChallengeChannelID;
        public readonly ulong ServerID;
        public readonly string Url = "https://hack.arrrg.de/";

        public Config(string username, string password, string token, ulong scoreboardChannelID, ulong challengeChannelID, ulong serverID)
        {
            Username = username;
            Password = password;
            Token = token;
            ScoreboardChannelID = scoreboardChannelID;
            ChallengeChannelID = challengeChannelID;
            ServerID = serverID;
        }
    }
}
