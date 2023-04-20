namespace HTWDiscordBot.Services
{
    //Stellt Konfigurationsdaten bereit
    public class ConfigService
    {
        public readonly Config Config;

        public ConfigService(string path = ".env")
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
            string[] content = File.ReadAllLines(path);
            Config = new(content[0].Trim(), content[1].Trim(), content[2].Trim(), ulong.Parse(content[3].Trim()), ulong.Parse(content[4].Trim()));
        }
    }

    public class Config
    {
        public readonly string Username;
        public readonly string Password;
        public readonly string Token;
        public readonly ulong ScoreboardChannelID;
        public readonly ulong ChallengeChannelID;
        public readonly string Url = "https://hack.arrrg.de/";

        public Config(string username, string password, string token, ulong scoreboardChannelID, ulong challengeChannelID)
        {
            Username = username;
            Password = password;
            Token = token;
            ScoreboardChannelID = scoreboardChannelID;
            ChallengeChannelID = challengeChannelID;
        }
    }
}
