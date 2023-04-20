using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;
using System.Text.Json;

namespace HTWDiscordBot.Services.HTW
{
    public class HTWUserService
    {
        private readonly HttpClientService httpService;
        private readonly HtmlParserService htmlParserService;
        private readonly AuthentificationService authentificationService;
        private readonly DiscordSocketClient client;
        private readonly ConfigService configService;
        //Schlüssel ist die Discord ID, Wert ist der HTW Username
        private readonly string path = "verifiedUsers.json";
        private Dictionary<ulong, string> verifiedUsers = new();

        public HTWUserService(HttpClientService httpService, HtmlParserService htmlParserService, AuthentificationService authentificationService, DiscordSocketClient client, ConfigService configService)
        {
            this.httpService = httpService;
            this.htmlParserService = htmlParserService;
            this.authentificationService = authentificationService;
            this.client = client;
            this.configService = configService;
        }

        public async Task InitializeAsync()
        {
            verifiedUsers = await ReadDictionaryAsync();
        }

        //Prüft ob einem Discord User der HTW Account wirklich gehört
        public async Task<string> IsRealUserAsync(string username, string password, ulong id)
        {
            Dictionary<string, string> requestContent = new()
            {
                { "username", username },
                { "password", password },
            };
            string authCookie = await authentificationService.GetAuthCookieAsync(requestContent);

            HttpRequestMessage requestMessage = new(HttpMethod.Get, "map");
            requestMessage.Headers.Add("Cookie", $"connect.sid={authCookie}");
            HttpResponseMessage responseMessage = await httpService.httpClient.SendAsync(requestMessage);

            //Der gefundene Username
            string? scrapedUsername = htmlParserService.ParseUsername(await responseMessage.Content.ReadAsStringAsync());

            if (scrapedUsername == null)
                return "Login unsuccessful";

            if (verifiedUsers.ContainsKey(id))
                verifiedUsers[id] = scrapedUsername;
            else
                verifiedUsers.Add(id, scrapedUsername);

            await WriteDictionaryAsync(verifiedUsers);
            await UpdateNicknames();
            return $"Login successful, {scrapedUsername}";
        }

        //Updatet die Nicknames aller Discord User die sich mit dem HTW Account verifiziert haben
        public async Task UpdateNicknames()
        {
            SocketGuild guild = client.GetGuild(configService.Config.ServerID);

            HttpRequestMessage requestMessage = new(HttpMethod.Get, "highscore");
            HttpResponseMessage responseMessage = await httpService.httpClient.SendAsync(requestMessage);
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
                if (user.Hierarchy > guild.GetUser(client.CurrentUser.Id).Hierarchy)
                {
                    await user.Guild.GetTextChannel(configService.Config.ChallengeChannelID).SendMessageAsync($"{MentionUtils.MentionUser(pair.Key)} Ich habe nicht genug Rechte um deinen Nicknamen zu ändern :(");
                    verifiedUsers.Remove(pair.Key);
                    await WriteDictionaryAsync(verifiedUsers);
                    continue;
                }
                await user.ModifyAsync(x => x.Nickname = $"{user.Username} #{playerData[0].InnerText}");
            }
        }

        //Schreibt das Dictionary mit den Accounts
        private async Task WriteDictionaryAsync(Dictionary<ulong, string> dict)
        {
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(dict));
        }

        //Liest das Dictionary mit den Accounts
        private async Task<Dictionary<ulong, string>> ReadDictionaryAsync()
        {
            if (!File.Exists(path))
            {
                await WriteDictionaryAsync(new Dictionary<ulong, string>());
            }
            Dictionary<ulong, string>? dict = await JsonSerializer.DeserializeAsync<Dictionary<ulong, string>>(File.OpenRead(path));

            if (dict == null)
                return new Dictionary<ulong, string>();

            return dict;
        }
    }
}
