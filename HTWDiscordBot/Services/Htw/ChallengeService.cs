using Discord;
using Discord.WebSocket;
using HtwDiscordBot.Models;
using Newtonsoft.Json;

namespace HtwDiscordBot.Services.Htw;

public class ChallengeService(
    DiscordSocketClient client,
    IHttpClientFactory httpClientFactory,
    LoggingService loggingService,
    ConfigService configService)
{
    private const string Path = "challengeMap.json";


    public async Task CheckForNewChallengeAsync(ulong textChannelID)
    {
        List<ChallengeModel> oldChallengeMap = await ReadChallengeMapAsync(Path);
        List<ChallengeModel>? newChallengeMap = await GetChallengeMapAsync();

        if (newChallengeMap == null)
        {
            await loggingService.LogAsync(new(LogSeverity.Error, "ChallengeService",
                "ChallengeMap konnte nicht geladen werden!"));
            return;
        }

        List<ChallengeModel> diff = await GetChallengeDiffAsync(oldChallengeMap, newChallengeMap);

        // if there are new challenges, write the new map to the file and notify the users
        if (diff.Count > 0)
        {
            await WriteChallengeMapAsync(newChallengeMap, Path);
            await NotifyNewChallengesAsync(diff, textChannelID);
        }
    }


    private async Task<List<ChallengeModel>?> GetChallengeMapAsync()
    {
        loggingService.Log(new(LogSeverity.Info, "ChallengeService", "Requesting api/map"));
        HttpClient httpClient = httpClientFactory.CreateClient("client");

        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/map");
        HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);
        string content = await responseMessage.Content.ReadAsStringAsync();
        try
        {
            List<int>? ids = JsonConvert.DeserializeObject<List<int>>(content);
            if (ids == null || ids.Count < 1)
                return null;

            return ids.Select(id => new ChallengeModel() { ID = id }).ToList();
        }
        catch (Exception ex)
        {
            await loggingService.LogAsync(new(LogSeverity.Error, "ChallengeService", ex.Message));
            await loggingService.LogAsync(new(LogSeverity.Error, "ChallengeService", content));
            return null;
        }
    }

    // notify the users about new challenges
    private async Task NotifyNewChallengesAsync(List<ChallengeModel> challenges, ulong textChannelID)
    {
        SocketTextChannel? textChannel = await client.GetChannelAsync(textChannelID) as SocketTextChannel;

        if (textChannel == null)
        {
            await loggingService.LogAsync(new(LogSeverity.Error, "ChallengeService",
                "TextChannel konnte nicht gefunden werden!"));
            return;
        }

        string message = $"{MentionUtils.MentionRole(configService.Config.ActiveHackerRoleID)} Neue Aufgaben:\n";
        foreach (ChallengeModel challenge in challenges.TakeLast(20))
        {
            message += $"{challenge.URL}\n";
            await loggingService.LogAsync(new(LogSeverity.Info, "ChallengeService",
                $"New challenge available: {challenge.URL}"));
        }

        await textChannel.SendMessageAsync(message);
    }

    private async Task<List<ChallengeModel>> GetChallengeDiffAsync(List<ChallengeModel> oldMap,
        List<ChallengeModel> newMap)
    {
        return newMap.Where(challenge => oldMap.All(oldChallenge => oldChallenge.ID != challenge.ID)).ToList();
    }

    private async Task<List<ChallengeModel>> ReadChallengeMapAsync(string path)
    {
        List<ChallengeModel>? challengeMap;
        if (!File.Exists(path))
        {
            challengeMap = await GetChallengeMapAsync();
            await WriteChallengeMapAsync(challengeMap, path);
            Console.WriteLine($"New {path} file generated.");

            if (challengeMap == null)
                return new List<ChallengeModel>();

            return challengeMap;
        }

        challengeMap = JsonConvert.DeserializeObject<List<ChallengeModel>>(await File.ReadAllTextAsync(path));
        return challengeMap ?? new List<ChallengeModel>();
    }

    private async Task WriteChallengeMapAsync(List<ChallengeModel>? challengeMap, string path)
    {
        await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(challengeMap, Formatting.Indented));
    }
}