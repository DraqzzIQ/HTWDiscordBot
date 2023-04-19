using Discord.WebSocket;
using System.Net;
using System.Text.RegularExpressions;

namespace HTWDiscordBot.Services
{
    //Stellt Funktionen bereit um auf die Hack The Web Seite zuzugreifen
    internal class HTWService
    {
        private readonly DiscordService discordService;
        private readonly ConfigService configService;
        private readonly HttpClientService httpService;
        private readonly HtmlParserService htmlParserService;
        private readonly Dictionary<string, string> requestContent = new();
        private readonly int delayInSeconds = 30;
        private readonly string path = "challengeID.txt";
        private bool shouldCheck = true;

        public HTWService(DiscordService discordService, ConfigService configService, HttpClientService httpService, HtmlParserService htmlParserService)
        {
            this.discordService = discordService;
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

        public async Task<string> GetPlayerDataAsync(SocketSlashCommandDataOption username)
        {
            HttpRequestMessage requestMessage = new(HttpMethod.Get, "highscore");
            HttpResponseMessage responseMessage = await httpService.httpClient.SendAsync(requestMessage);

            return htmlParserService.GetPlayerData(await responseMessage.Content.ReadAsStringAsync(),username.Value.ToString());
        }

        //Läuft jede 30 Sekunden um auf neue Aufgaben zu prüfen
        private async Task LoopAsync()
        {
            while (true)
            {
                if (shouldCheck)
                {
                    await CheckForNewChallenge();
                }
                await Task.Delay(delayInSeconds * 1000);
            }
        }

        //Überprüft ob neue Aufgaben vorhanden sind
        private async Task CheckForNewChallenge()
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
                challengeID++;
                await WriteChallengeIDAsync(challengeID);
                await UpdateAsync($"{configService.Config.Url}challenge/{challengeID}");
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
        private async Task UpdateAsync(string url)
        {
            foreach (SocketGuild socketGuild in discordService.client.Guilds)
            {
                if (!socketGuild.TextChannels.Any(c => c.Name == "htwbot"))
                    await socketGuild.CreateTextChannelAsync("htwbot").Result.SendMessageAsync($"@everyone Neue Aufgabe: {url}");
                else
                    foreach (SocketTextChannel textChannel in socketGuild.TextChannels)
                    {
                        if (textChannel.Name == "htwbot")
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
    }
}
