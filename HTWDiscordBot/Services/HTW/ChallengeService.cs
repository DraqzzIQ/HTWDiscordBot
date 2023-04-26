using System.Net;
using Discord;
using Discord.WebSocket;

namespace HTWDiscordBot.Services.HTW
{
    //Challenge Logik
    public class ChallengeService
    {
        private readonly DiscordSocketClient client;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly AuthentificationService authentificationService;
        private readonly ConfigService configService;
        private readonly LoggingService loggingService;
        private readonly string path = "challengeID.txt";

        public ChallengeService(DiscordSocketClient client, IHttpClientFactory httpClientFactory, AuthentificationService authentificationService, ConfigService configService, LoggingService loggingService)
        {
            this.client = client;
            this.httpClientFactory = httpClientFactory;
            this.authentificationService = authentificationService;
            this.configService = configService;
            this.loggingService = loggingService;
        }

        //Überprüft ob neue Aufgaben vorhanden sind
        public async Task CheckForNewChallengeAsync(Dictionary<string, string> requestContent, ulong textChannelID)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("client");

            string authCookie = await authentificationService.GetAuthCookieAsync(requestContent);
            int challengeID = await ReadChallengeIDAsync();

            //Checkt ob neue Aufgabe vorhanden ist
            HttpRequestMessage requestMessage = new(HttpMethod.Get, $"challenge/{challengeID}");
            requestMessage.Headers.Add("Cookie", $"connect.sid={authCookie}");
            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);

            //Wenn neue Aufgabe vorhanden ist, lädt die Seite (HttpStatusCode.OK)
            if (responseMessage.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("New challenge available");
                await WriteChallengeIDAsync(challengeID + 1);
                await UpdateChallengeAsync($"{Config.Url}challenge/{challengeID}", textChannelID);
            }
            //Wenn keine neue Aufgabe vorhanden ist, wird man auf die Startseite weitergeleitet (HttpStatusCode.Redirect)
            else if (responseMessage.StatusCode == HttpStatusCode.Redirect)
            {
            }
        }

        //Benachrichtigt alle Discord Server auf denen der Bot sich befindet, dass es eine neue Aufgabe gibt
        private async Task UpdateChallengeAsync(string url, ulong textChannelID)
        {
            SocketTextChannel? textChannel = await client.GetChannelAsync(textChannelID) as SocketTextChannel;

            if (textChannel == null)
            {
                await loggingService.LogAsync(new(LogSeverity.Error, "ChallengeService", "TextChannel konnte nicht gefunden werden!"));
                return;
            }

            await textChannel.SendMessageAsync($"@everyone Neue Aufgabe: {url}");
        }

        //Liest die Challenge ID aus challengeID.txt
        private async Task<int> ReadChallengeIDAsync()
        {
            //1. Zeile ChallengeID z.B. 69
            if (!File.Exists(path))
            {
                File.Create(path);
                Console.WriteLine($"New {path} file generated. Please configure and restart");
                Environment.Exit(-1);
            }

            string id = await File.ReadAllTextAsync(path);
            int challengeID = int.Parse(id.Trim());

            return challengeID;
        }

        //Schreibt die Challenge ID in die challengeID.txt
        private async Task WriteChallengeIDAsync(int challengeID)
        {
            await File.WriteAllTextAsync(path, challengeID.ToString());
        }
    }
}