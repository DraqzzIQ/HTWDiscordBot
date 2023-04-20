using Discord.WebSocket;
using HTWDiscordBot.Services.HTW;

namespace HTWDiscordBot.Services
{
    //Verwaltet die HTW Logik
    public class HTWService
    {
        private readonly DiscordSocketClient client;
        private readonly ConfigService configService;
        private readonly AuthentificationService authentificationService;
        private readonly ScoreboardService scoreboardService;
        private readonly ChallengeService challengeService;
        private readonly Dictionary<string, string> requestContent = new();
        private readonly int delayInSeconds = 30;

        public HTWService(DiscordSocketClient client, ConfigService configService, AuthentificationService authentificationService, ScoreboardService scoreboardService, ChallengeService challengeService)
        {
            this.client = client;
            this.configService = configService;
            this.authentificationService = authentificationService;
            this.scoreboardService = scoreboardService;
            this.challengeService = challengeService;
        }

        public Task InitializeAsync()
        {
            Task.Run(() => LoopAsync());
            requestContent.Add("username", configService.Config.Username);
            requestContent.Add("password", configService.Config.Password);
            return Task.CompletedTask;
        }

        //Läuft jede 30 Sekunden um auf neue Aufgaben zu prüfen
        private async Task LoopAsync()
        {
            while (true)
            {
                await challengeService.CheckForNewChallengeAsync(requestContent, configService.Config.ChallengeChannelID);
                await scoreboardService.UpdateScoreboardAsync(configService.Config.ScoreboardChannelID);
                await Task.Delay(delayInSeconds * 1000);
            }
        }
    }
}