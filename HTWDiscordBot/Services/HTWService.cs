using Discord.WebSocket;
using System.Net;
using System.Text.RegularExpressions;

namespace HTWDiscordBot.Services
{
    internal class HTWService
    {
        private DiscordService discordService;
        ConfigService configService;
        Uri url = new("https://hack.arrrg.de/");
        Dictionary<string, string> requestContent = new();
        private bool shouldCheck = true;
        private int delayInSeconds = 30;
        private string path = "challengeID.txt";
        private int challengeID;

        public HTWService(DiscordService discordService, ConfigService configService)
        {
            this.discordService = discordService;
            this.configService = configService;
        }

        public Task InitializeAsync()
        {
            Task.Run(() => CheckForNewHTWTask());
            requestContent.Add("username", configService.Config.Username);
            requestContent.Add("password", configService.Config.Password);
            return Task.CompletedTask;
        }

        private async Task ReadChallengeID()
        {
            //1. Zeile ChallengeID z.B. 69
            if (!File.Exists(path))
            {
                File.Create(path);
                Console.WriteLine($"New {path} file generated. Please configure and restart");
                Environment.Exit(-1);
            }

            string id = await File.ReadAllTextAsync(path);
            challengeID = int.Parse(id.Trim());
        }
        private async Task WriteChallengeID()
        {
            await File.WriteAllTextAsync(path, challengeID.ToString());
        }

        public Task SetShouldCheck(bool shouldCheck)
        {
            this.shouldCheck = shouldCheck;
            return Task.CompletedTask;
        }

        private async Task Update(string url)
        {
            foreach (SocketGuild socketGuild in discordService.client.Guilds)
            {
                if (!socketGuild.TextChannels.Any(c => c.Name == "htwbot"))
                    await socketGuild.CreateTextChannelAsync("htwbot").Result.SendMessageAsync($"@everyone Neue Aufgabe: {url}");
                else
                    foreach (var textChannel in socketGuild.TextChannels)
                    {
                        if (textChannel.Name == "htwbot")
                        {
                            await textChannel.SendMessageAsync($"@everyone Neue Aufgabe: {url}");
                            break;
                        }
                    }
            }
        }

        private async Task CheckForNewHTWTask()
        {
            while (true)
            {
                if (shouldCheck)
                {
                    await ReadChallengeID();
                    await Check();
                }
                await Task.Delay(delayInSeconds * 1000);
            }
        }

        private async Task Check()
        {
            string authCookie = "";

            using (var handler = new HttpClientHandler { UseCookies = false, AllowAutoRedirect = false })
            using (var client = new HttpClient(handler) { BaseAddress = url })
            {
                //HttpRequestMessage um den session id cookie zu bekommen
                var message = new HttpRequestMessage(HttpMethod.Post, "login");

                //Fügt die login Daten hinzu
                message.Content = new FormUrlEncodedContent(requestContent);
                var responseMessage = client.Send(message);

                foreach (string cookie in responseMessage.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value)
                    if (cookie.Contains("connect.sid"))
                        authCookie = ExtractSessionId(cookie);


                //Checkt ob neue Aufgabe vorhanden ist
                message = new HttpRequestMessage(HttpMethod.Get, $"challenge/{challengeID}");
                message.Headers.Add("Cookie", $"connect.sid={authCookie}");
                responseMessage = client.Send(message);

                //Wenn neue Aufgabe vorhanden ist, lädt die Seite (HttpStatusCode.OK)
                if (responseMessage.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("New challenge available");
                    challengeID++;
                    await WriteChallengeID();
                    await Update($"{url.OriginalString}challenge/{challengeID}");
                }
                //Wenn keine neue Aufgabe vorhanden ist, wird man auf die Startseite weitergeleitet (HttpStatusCode.Redirect)
                else if (responseMessage.StatusCode == HttpStatusCode.Redirect)
                {
                }
            }
        }

        static string ExtractSessionId(string cookieString)
        {
            Regex regex = new Regex(@"connect\.sid=([^;]+);");
            Match match = regex.Match(cookieString);
            return match.Groups[1].Value;
        }
    }
}
