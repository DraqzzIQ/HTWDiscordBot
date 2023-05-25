using Discord;
using Discord.WebSocket;
using HTWDiscordBot.Models;
using Newtonsoft.Json;

namespace HTWDiscordBot.Services.HTW
{
    //Challenge Logik
    public class ChallengeService
    {
        private readonly DiscordSocketClient client;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly LoggingService loggingService;
        private readonly ConfigService configService;
        private readonly string path = "challengeMap.json";


        public ChallengeService(DiscordSocketClient client, IHttpClientFactory httpClientFactory, LoggingService loggingService, ConfigService configService)
        {
            this.client = client;
            this.httpClientFactory = httpClientFactory;
            this.loggingService = loggingService;
            this.configService = configService;
        }

        //Überprüft ob neue Aufgaben vorhanden sind
        public async Task CheckForNewChallengeAsync(ulong textChannelID)
        {
            List<ChallengeModel> oldChallengeMap = await ReadChallengeMapAsync(path);
            List<ChallengeModel>? newChallengeMap = await GetChallengeMapAsync();

            if (newChallengeMap == null)
            {
                await loggingService.LogAsync(new(LogSeverity.Error, "ChallengeService", "ChallengeMap konnte nicht geladen werden!"));
                return;
            }

            List<ChallengeModel> diff = await GetChallengeDiffAsync(oldChallengeMap, newChallengeMap);

            //Wenn neue Aufgabe vorhanden ist
            if (diff.Count > 0)
            {
                await WriteChallengeMapAsync(newChallengeMap, path);
                await NotifyNewChallengesAsync(diff, textChannelID);
            }
        }


        private async Task<List<ChallengeModel>?> GetChallengeMapAsync()
        {
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

        //Benachrichtigt User darüber, dass es eine neue Aufgabe gibt
        private async Task NotifyNewChallengesAsync(List<ChallengeModel> challenges, ulong textChannelID)
        {
            SocketTextChannel? textChannel = await client.GetChannelAsync(textChannelID) as SocketTextChannel;

            if (textChannel == null)
            {
                await loggingService.LogAsync(new(LogSeverity.Error, "ChallengeService", "TextChannel konnte nicht gefunden werden!"));
                return;
            }

            string message = $"{MentionUtils.MentionRole(configService.Config.RoleID)} Neue Aufgaben:\n";
            foreach (ChallengeModel challenge in challenges.TakeLast(20))
            {
                message += $"{challenge.URL}\n";
                await loggingService.LogAsync(new(LogSeverity.Info, "ChallengeService", $"New challenge available: {challenge.URL}"));
            }
            await textChannel.SendMessageAsync(message);
        }

        private async Task<List<ChallengeModel>> GetChallengeDiffAsync(List<ChallengeModel> oldMap, List<ChallengeModel> newMap)
        {
            List<ChallengeModel> diff = new();

            foreach (ChallengeModel challenge in newMap)
            {
                if (!oldMap.Any(oldChallenge => oldChallenge.ID == challenge.ID))
                    diff.Add(challenge);
            }

            return diff;
        }

        //Liest die ChallengeMap aus einer Textdatei
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
            if (challengeMap == null)
                return new List<ChallengeModel>();
            return challengeMap;
        }

        //Schreibt die ChallengeMap in einer Textdatei
        private async Task WriteChallengeMapAsync(List<ChallengeModel>? challengeMap, string path)
        {
            await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(challengeMap, Formatting.Indented));
        }
    }
}