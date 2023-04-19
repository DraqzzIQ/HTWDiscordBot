using Discord;
using Discord.WebSocket;
using System.Net;
using System.Text.RegularExpressions;

namespace HTWDiscordBot.Services
{
    //Stellt Funktionen bereit um auf die Hack The Web Seite zuzugreifen
    internal class HTWService
    {
        private readonly DiscordSocketClient client;
        private readonly ConfigService configService;
        private readonly HttpClientService httpService;
        private readonly HtmlParserService htmlParserService;
        private readonly Dictionary<string, string> requestContent = new();
        private readonly int delayInSeconds = 30;
        private readonly string path = "challengeID.txt";
        private bool shouldCheck = true;
        private readonly string challengeUpdateChannel = "htwbot";
        private readonly string scoreboardChannel = "scoreboard";
        private string cachedScoreboard = "";

        public HTWService(DiscordSocketClient client, ConfigService configService, HttpClientService httpService, HtmlParserService htmlParserService)
        {
            this.client = client;
            this.configService = configService;
            this.httpService = httpService;
            this.htmlParserService = htmlParserService;
        }

        public Task InitializeAsync()
        {
            Task.Run(() => LoopAsync());
            requestContent.Add("username", configService.Config.Username);
            requestContent.Add("password", configService.Config.Password);
            return Task.CompletedTask;
        }

        //Aktiviert oder deaktiviert die Überprüfung auf neue Aufgaben
        public Task SetShouldCheckAsync(bool shouldCheck)
        {
            this.shouldCheck = shouldCheck;
            return Task.CompletedTask;
        }

        //Gibt ein Scoreboard als String zurück
        public async Task<string> GetScoreboardAsync()
        {
            HttpRequestMessage requestMessage = new(HttpMethod.Get, "highscore");
            HttpResponseMessage responseMessage = await httpService.httpClient.SendAsync(requestMessage);

            return htmlParserService.ParseScoreBoard(await responseMessage.Content.ReadAsStringAsync());
        }

        //Läuft jede 30 Sekunden um auf neue Aufgaben zu prüfen
        private async Task LoopAsync()
        {
            while (true)
            {
                if (shouldCheck)
                {
                    await CheckForNewChallengeAsync();
                    await CheckForScoreboardChangeAsync(await GetScoreboardAsync());
                }
                await Task.Delay(delayInSeconds * 1000);
            }
        }

        //Überprüft, ob das Scoreboard geändert wurde
        private async Task CheckForScoreboardChangeAsync(string scoreboard)
        {
            if (cachedScoreboard != scoreboard)
            {
                cachedScoreboard = scoreboard;
            }
            await UpdateScoreboardAsync(scoreboard);
        }

        //Updated das Scoreboard
        private async Task UpdateScoreboardAsync(string scoreboard)
        {
            Console.WriteLine("Updating Scoreboard");

            foreach (SocketGuild socketGuild in client.Guilds)
            {
                if (!socketGuild.TextChannels.Any(c => c.Name == scoreboardChannel))
                    await socketGuild.CreateTextChannelAsync(scoreboardChannel);

                foreach (SocketTextChannel textChannel in socketGuild.TextChannels)
                {
                    if (textChannel.Name == scoreboardChannel)
                    {
                        IMessage? message = textChannel.GetMessagesAsync(1).FlattenAsync().Result.FirstOrDefault();

                        if (message == null || message.Author.Id != client.CurrentUser.Id)
                            await textChannel.SendMessageAsync(embed: CreateScoreboardEmbed(scoreboard));
                        else
                            await textChannel.ModifyMessageAsync(message.Id, m => m.Embed = CreateScoreboardEmbed(scoreboard));

                        break;
                    }
                }
            }
        }

        //Überprüft ob neue Aufgaben vorhanden sind
        private async Task CheckForNewChallengeAsync()
        {
            string authCookie = await GetAuthCookieAsync();
            int challengeID = await ReadChallengeIDAsync();

            //Checkt ob neue Aufgabe vorhanden ist
            HttpRequestMessage requestMessage = new(HttpMethod.Get, $"challenge/{challengeID}");
            requestMessage.Headers.Add("Cookie", $"connect.sid={authCookie}");
            HttpResponseMessage responseMessage = await httpService.httpClient.SendAsync(requestMessage);

            //Wenn neue Aufgabe vorhanden ist, lädt die Seite (HttpStatusCode.OK)
            if (responseMessage.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("New challenge available");
                await WriteChallengeIDAsync(challengeID + 1);
                await UpdateChallengeAsync($"{configService.Config.Url}challenge/{challengeID}");
            }
            //Wenn keine neue Aufgabe vorhanden ist, wird man auf die Startseite weitergeleitet (HttpStatusCode.Redirect)
            else if (responseMessage.StatusCode == HttpStatusCode.Redirect)
            {
            }
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

        //Benachrichtigt alle Discord Server auf denen der Bot sich befindet, dass es eine neue Aufgabe gibt
        private async Task UpdateChallengeAsync(string url)
        {
            foreach (SocketGuild socketGuild in client.Guilds)
            {
                if (!socketGuild.TextChannels.Any(c => c.Name == challengeUpdateChannel))
                    await socketGuild.CreateTextChannelAsync(challengeUpdateChannel);

                foreach (SocketTextChannel textChannel in socketGuild.TextChannels)
                {
                    if (textChannel.Name == challengeUpdateChannel)
                    {
                        await textChannel.SendMessageAsync($"@everyone Neue Aufgabe: {url}");
                        break;
                    }
                }
            }
        }

        //Authentifiziert sich mit konfiguriertem Nutzername und Passwort und gibt den Session Cookie zurück
        private async Task<string> GetAuthCookieAsync()
        {
            string authCookie = "";

            //HttpRequestMessage um den session id cookie zu bekommen
            HttpRequestMessage requestMessage = new(HttpMethod.Post, "login");

            //Fügt die login Daten hinzu
            requestMessage.Content = new FormUrlEncodedContent(requestContent);
            HttpResponseMessage responseMessage = await httpService.httpClient.SendAsync(requestMessage);

            //Liest den Cookie mit der session id aus
            foreach (string cookie in responseMessage.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value)
                if (cookie.Contains("connect.sid"))
                    authCookie = ExtractSessionId(cookie);
            return authCookie;
        }

        //Nutzt Regex um den Cookie zu extrahieren
        private static string ExtractSessionId(string cookieString)
        {
            Regex regex = new(@"connect\.sid=([^;]+);");
            Match match = regex.Match(cookieString);
            return match.Groups[1].Value;
        }

        //Erstellt einen Embed mit dem Scoreboard
        private static Embed CreateScoreboardEmbed(string scoreboard)
        {
            EmbedBuilder embedBuilder = new EmbedBuilder()
            .WithTitle("Scoreboard")
            .WithDescription(scoreboard)
            .WithColor(Color.Blue)
            .WithCurrentTimestamp();
            return embedBuilder.Build();
        }
    }
}