namespace HTWDiscordBot.Services
{
    //Stellt Konfigurationsdaten bereit
    internal class ConfigService
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
            string[] content = File.ReadAllLines(path);
            Config = new(content[0].Trim(), content[1].Trim(), content[2].Trim());
        }
    }

    internal class Config
    {
        public readonly string Username;
        public readonly string Password;
        public readonly string Token;
        public readonly string Url = "https://hack.arrrg.de/";

        public Config(string username, string password, string token)
        {
            Username = username;
            Password = password;
            Token = token;
        }
    }
}
