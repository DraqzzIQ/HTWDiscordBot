using Discord;

namespace HtwDiscordBot.Services.Htw;

public class HtwService(
    ConfigService configService,
    ScoreboardService scoreboardService,
    ChallengeService challengeService,
    HtwUserService htwUserService,
    LoggingService loggingService)
{
    private const int ChallengeCheckDelayInSeconds = 30;
    private const int NicknameUpdateDelayInSeconds = 5 * 60;
    private const int ScoreboardUpdateDelayInSeconds = 5 * 60;


    public Task InitializeAsync()
    {
        Task.Run(ChallengeLoopAsync).ContinueWith(t =>
            loggingService.Log(
                new LogMessage(LogSeverity.Critical, "HtwService ChallengeLoop", t.Exception?.ToString())));
        Task.Run(ScoreboardLoopAsync).ContinueWith(t =>
            loggingService.Log(new LogMessage(LogSeverity.Critical, "HtwService ScoreboardLoop",
                t.Exception?.ToString())));
        Task.Run(NicknameLoopAsync).ContinueWith(t =>
            loggingService.Log(new LogMessage(LogSeverity.Critical, "HtwService NicknameLoop",
                t.Exception?.ToString())));

        return Task.CompletedTask;
    }

    // check for new challenges every 30 seconds
    private async Task ChallengeLoopAsync()
    {
        while (true)
        {
            try
            {
                await Task.Delay(ChallengeCheckDelayInSeconds * 1000);
                await challengeService.CheckForNewChallengeAsync(configService.Config.ChallengeChannelID);
            }
            catch (Exception e)
            {
                await loggingService.LogAsync(new LogMessage(LogSeverity.Critical, "HtwService ChallengeLoop",
                    e.ToString()));
            }
        }
    }

    // update the scoreboard every 5 minutes
    private async Task ScoreboardLoopAsync()
    {
        while (true)
        {
            try
            {
                await Task.Delay(ScoreboardUpdateDelayInSeconds * 1000);
                await scoreboardService.UpdateScoreboardAsync(configService.Config.ScoreboardChannelID);
            }
            catch (Exception e)
            {
                await loggingService.LogAsync(new LogMessage(LogSeverity.Critical, "HtwService ScoreboardLoop",
                    e.ToString()));
            }
        }
    }

    // update nicknames every 5 minutes
    private async Task NicknameLoopAsync()
    {
        while (true)
        {
            try
            {
                await Task.Delay(NicknameUpdateDelayInSeconds * 1000);
                await htwUserService.UpdateNicknames();
            }
            catch (Exception e)
            {
                await loggingService.LogAsync(new LogMessage(LogSeverity.Critical, "HtwService NicknameLoop",
                    e.ToString()));
            }
        }
    }
}