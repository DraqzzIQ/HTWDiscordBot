using Discord.WebSocket;
using System.Net;
using System.Text.RegularExpressions;

namespace HTWDiscordBot.Services
{
    //Klasse um auf die Hack The Web Seite zuzugreifen
    internal class HTWService
    {
        private readonly DiscordService discordService;
        private readonly ConfigService configService;
        private readonly HttpClientService httpService;
        private readonly Uri url = new("https://hack.arrrg.de/");
        private readonly Dictionary<string, string> requestContent = new();
        private readonly int delayInSeconds = 30;
        private readonly string path = "challengeID.txt";
        private bool shouldCheck = true;


        public HTWService(DiscordService discordService, ConfigService configService, HttpClientService httpService)
        {
            this.discordService = discordService;
            this.configService = configService;
            this.httpService = httpService;
        }

        public Task InitializeAsync()
        {
            Task.Run(() => Loop());
            requestContent.Add("username", configService.Config.Username);
            requestContent.Add("password", configService.Config.Password);
            return Task.CompletedTask;
        }

        //Aktiviert oder deaktiviert die Überprüfung auf neue Aufgaben
        public Task SetShouldCheck(bool shouldCheck)
        {
            this.shouldCheck = shouldCheck;
            return Task.CompletedTask;
        }

        //Läuft jede 30 Sekunden um auf neue Aufgaben zu prüfen
        private async Task Loop()
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
            string authCookie = await GetAuthCookie();
            int challengeID = await ReadChallengeID();

            //Checkt ob neue Aufgabe vorhanden ist
            HttpRequestMessage message = new(HttpMethod.Get, $"challenge/{challengeID}");
            message.Headers.Add("Cookie", $"connect.sid={authCookie}");
            HttpResponseMessage responseMessage = await httpService.httpClient.SendAsync(message);

            //Wenn neue Aufgabe vorhanden ist, lädt die Seite (HttpStatusCode.OK)
            if (responseMessage.StatusCode == HttpStatusCode.OK)
            {
                Console.WriteLine("New challenge available");
                challengeID++;
                await WriteChallengeID(challengeID);
                await Update($"{url.OriginalString}challenge/{challengeID}");
            }
            //Wenn keine neue Aufgabe vorhanden ist, wird man auf die Startseite weitergeleitet (HttpStatusCode.Redirect)
            else if (responseMessage.StatusCode == HttpStatusCode.Redirect)
            {
            }
        }

        //Liest die Challenge ID aus challengeID.txt
        private async Task<int> ReadChallengeID()
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
        private async Task WriteChallengeID(int challengeID)
        {
            await File.WriteAllTextAsync(path, challengeID.ToString());
        }

        //Benachrichtigt alle Discord Server auf denen der Bot sich befindet, dass es eine neue Aufgabe gibt
        private async Task Update(string url)
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
        private async Task<string> GetAuthCookie()
        {
            string authCookie = "";

            //HttpRequestMessage um den session id cookie zu bekommen
            HttpRequestMessage message = new(HttpMethod.Post, "login");

            //Fügt die login Daten hinzu
            message.Content = new FormUrlEncodedContent(requestContent);
            HttpResponseMessage responseMessage = await httpService.httpClient.SendAsync(message);

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
