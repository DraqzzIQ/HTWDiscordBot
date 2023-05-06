using Discord;
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
        private readonly LoggingService loggingService;
        private readonly Dictionary<string, string> requestContent = new();
        private readonly int delayInSeconds = 30;

        public HTWService(ConfigService configService, ScoreboardService scoreboardService, ChallengeService challengeService, HTWUserService htwUserService, LoggingService loggingService)
        {
            this.configService = configService;
            this.scoreboardService = scoreboardService;
            this.challengeService = challengeService;
            this.htwUserService = htwUserService;
            this.loggingService = loggingService;
        }

        public Task InitializeAsync()
        {
            requestContent.Add("username", configService.Config.Username);
            requestContent.Add("password", configService.Config.Password);

            Task.Run(() => LoopAsync()).ContinueWith(t => loggingService.Log(new(LogSeverity.Critical, "HTWService Loop", t.Exception?.ToString())));
            return Task.CompletedTask;
        }

        //Läuft jede 30 Sekunden um auf neue Aufgaben zu prüfen
        private async Task LoopAsync()
        {
            while (true)
            {
                try
                {
                    await challengeService.CheckForNewChallengeAsync(requestContent, configService.Config.ChallengeChannelID);
                    await scoreboardService.UpdateScoreboardAsync(configService.Config.ScoreboardChannelID);
                    await htwUserService.UpdateNicknames();
                    await Task.Delay(delayInSeconds * 1000);
                }
                catch (Exception e)
                {
                    loggingService.Log(new(LogSeverity.Critical, "HTWService Loop", e.ToString()));
                }
            }
        }
    }
}