namespace HTWDiscordBot.Services
{
    internal class ConfigService
    {
        private readonly string path;
        public Config Config { get; private set; }

        public ConfigService(string path = ".env")
        {
            this.path = path;
        }

        public async Task InitializeAsync()
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
            string[] content = await File.ReadAllLinesAsync(path);
            Config = new(content[0].Trim(), content[1].Trim(), content[2].Trim());
        }
    }

    internal class Config
    {
        public readonly string Username;
        public readonly string Password;
        public readonly string Token;

        public Config(string username, string password, string token)
        {
            Username = username;
            Password = password;
            Token = token;
        }
    }
}
