using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace HTWDiscordBot.Services.HTW
{
    public class HTWUserService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly HtmlParserService htmlParserService;
        private readonly DiscordSocketClient client;
        private readonly ConfigService configService;
        //Schlüssel ist die Discord ID, Wert ist der HTW Username
        private readonly string path = "verifiedUsers.json";
        private Dictionary<ulong, string> verifiedUsers = new();

        public HTWUserService(IHttpClientFactory httpClientFactory, HtmlParserService htmlParserService, DiscordSocketClient client, ConfigService configService)
        {
            this.httpClientFactory = httpClientFactory;
            this.htmlParserService = htmlParserService;
            this.client = client;
            this.configService = configService;
        }

        public async Task InitializeAsync()
        {
            verifiedUsers = await ReadDictionaryAsync();
        }

        //Prüft ob einem Discord User der HTW Account wirklich gehört
        public async Task<string> IsRealUserAsync(string username, string token, ulong id)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("client");

            HttpRequestMessage requestMessage = new(HttpMethod.Get, $"verify/{username}/{token}");
            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);

            string? response = await responseMessage.Content.ReadAsStringAsync();

            if (response == null || response == "not valid")
                return "Login war nicht erfolgreich. Bitte überprüfe Benutzername und Token und versuche es erneut";

            if (verifiedUsers.ContainsKey(id))
                verifiedUsers[id] = username;
            else
                verifiedUsers.Add(id, username);

            await WriteDictionaryAsync(verifiedUsers);
            await UpdateNicknames();
            return $"Login erfolgreich, {username}";
        }

        //Loggt einen Benutzer aus
        public async Task LogoutAsync(ulong id)
        {
            if (!verifiedUsers.ContainsKey(id))
                return;

            SocketGuild guild = client.GetGuild(configService.Config.ServerID);
            SocketGuildUser? user = guild.GetUser(id);

            await user.ModifyAsync(x => x.Nickname = user.Username);

            verifiedUsers.Remove(id);
            await WriteDictionaryAsync(verifiedUsers);
        }

        //Updatet die Nicknames aller Discord User die sich mit dem HTW Account verifiziert haben
        public async Task UpdateNicknames()
        {
            HttpClient httpClient = httpClientFactory.CreateClient("client");

            SocketGuild guild = client.GetGuild(configService.Config.ServerID);

            HttpRequestMessage requestMessage = new(HttpMethod.Get, "highscore");
            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);
            string html = await responseMessage.Content.ReadAsStringAsync();

            KeyValuePair<ulong, string>[] pairs = verifiedUsers.ToArray();
            for (int i = 0; i < verifiedUsers.Count; i++)
            {
                KeyValuePair<ulong, string> pair = pairs[i];

                SocketGuildUser? user = guild.GetUser(pair.Key);
                List<HtmlNode>? playerData = htmlParserService.GetScoreBoardEntry(html, pair.Value);
                if (user == null || playerData == null)
                {
                    verifiedUsers.Remove(pair.Key);
                    await WriteDictionaryAsync(verifiedUsers);
                    continue;
                }
                //Bot hat nicht genug Rechte
                if (user.Hierarchy >= guild.GetUser(client.CurrentUser.Id).Hierarchy)
                {
                    await user.Guild.GetTextChannel(configService.Config.ChallengeChannelID).SendMessageAsync($"{MentionUtils.MentionUser(pair.Key)} Ich habe nicht genug Rechte um deinen Nicknamen zu ändern :(");
                    verifiedUsers.Remove(pair.Key);
                    await WriteDictionaryAsync(verifiedUsers);
                    continue;
                }
                await user.ModifyAsync(x => x.Nickname = $"{user.Username} #{playerData[0].InnerText}");
                await Task.Delay(1000); //Discord API Rate Limiter wird sonst sauer
            }
        }

        //Schreibt das Dictionary mit den Accounts
        private async Task WriteDictionaryAsync(Dictionary<ulong, string> dict)
        {
            await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(dict, formatting: Formatting.Indented));
        }

        //Liest das Dictionary mit den Accounts
        private async Task<Dictionary<ulong, string>> ReadDictionaryAsync()
        {
            if (!File.Exists(path))
            {
                await WriteDictionaryAsync(new Dictionary<ulong, string>());
            }
            Dictionary<ulong, string>? dict = JsonConvert.DeserializeObject<Dictionary<ulong, string>>(await File.ReadAllTextAsync(path));

            if (dict == null)
                return new Dictionary<ulong, string>();

            return dict;
        }
    }
}
