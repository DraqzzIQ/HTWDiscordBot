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
        private readonly int challengeCheckDelayInSeconds = 30;
        private readonly int nicknameUpdateDelayInSeconds = 5 * 60;
        private readonly int scoreboardUpdateDelayInSeconds = 5 * 60;


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
            Task.Run(() => ChallengeLoopAsync()).ContinueWith(t => loggingService.Log(new(LogSeverity.Critical, "HTWService ChallengeLoop", t.Exception?.ToString())));
            Task.Run(() => ScoreboardLoopAsync()).ContinueWith(t => loggingService.Log(new(LogSeverity.Critical, "HTWService ScoreboardLoop", t.Exception?.ToString())));
            Task.Run(() => NicknameLoopAsync()).ContinueWith(t => loggingService.Log(new(LogSeverity.Critical, "HTWService NicknameLoop", t.Exception?.ToString())));

            return Task.CompletedTask;
        }

        //Läuft jede 30 Sekunden um auf neue Aufgaben zu prüfen
        private async Task ChallengeLoopAsync()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(challengeCheckDelayInSeconds * 1000);
                    await challengeService.CheckForNewChallengeAsync(configService.Config.ChallengeChannelID);
                }
                catch (Exception e)
                {
                    loggingService.Log(new(LogSeverity.Critical, "HTWService ChallengeLoop", e.ToString()));
                }
            }
        }

        // Läuft jede 5 Minuten um das Scoreboard zu aktualisieren
        private async Task ScoreboardLoopAsync()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(scoreboardUpdateDelayInSeconds * 1000);
                    await scoreboardService.UpdateScoreboardAsync(configService.Config.ScoreboardChannelID);
                }
                catch (Exception e)
                {
                    loggingService.Log(new(LogSeverity.Critical, "HTWService ScoreboardLoop", e.ToString()));
                }
            }
        }

        // Läuft jede 5 Minuten um Nicknames zu aktualisieren
        private async Task NicknameLoopAsync()
        {
            while (true)
            {
                try
                {
                    await Task.Delay(nicknameUpdateDelayInSeconds * 1000);
                    await htwUserService.UpdateNicknames();
                }
                catch (Exception e)
                {
                    loggingService.Log(new(LogSeverity.Critical, "HTWService NicknameLoop", e.ToString()));
                }
            }
        }
    }
}