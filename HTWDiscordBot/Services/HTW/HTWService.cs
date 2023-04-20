using HTWDiscordBot.Services.HTW;

namespace HTWDiscordBot.Services
{
    //Verwaltet die HTW Logik
    public class HTWService
    {
        private readonly ConfigService configService;
        private readonly ScoreboardService scoreboardService;
        private readonly ChallengeService challengeService;
        private readonly HTWUserService htwUserService;
        private readonly Dictionary<string, string> requestContent = new();
        private readonly int delayInSeconds = 30;

        public HTWService(ConfigService configService, ScoreboardService scoreboardService, ChallengeService challengeService, HTWUserService htwUserService)
        {
            this.configService = configService;
            this.scoreboardService = scoreboardService;
            this.challengeService = challengeService;
            this.htwUserService = htwUserService;
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
                await htwUserService.UpdateNicknames();
                await Task.Delay(delayInSeconds * 1000);
            }
        }
    }
}