using Discord;
using Discord.WebSocket;
using HTWDiscordBot.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace HTWDiscordBot.Services.HTW
{
    public class HTWUserService
    {
        private readonly IHttpClientFactory httpClientFactory;
        private readonly DiscordSocketClient client;
        private readonly ConfigService configService;
        private readonly ScoreboardService scoreboardService;
        private readonly LoggingService loggingService;
        //Schlüssel ist die Discord ID, Wert ist der HTW Username
        private readonly string path = "verifiedUsers.json";
        private Dictionary<ulong, string> verifiedUsers = new();

        public HTWUserService(IHttpClientFactory httpClientFactory, DiscordSocketClient client, ConfigService configService, ScoreboardService scoreboardService, LoggingService loggingService)
        {
            this.httpClientFactory = httpClientFactory;
            this.scoreboardService = scoreboardService;
            this.client = client;
            this.configService = configService;
            this.loggingService = loggingService;
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

            SocketGuildUser[] socketUsers = guild.Users.ToArray();

            Dictionary<string, string> values = new();
            for (int i = 0; i < verifiedUsers.Count; i++)
            {
                KeyValuePair<ulong, string> pair = verifiedUsers.ElementAt(i);
                values.Add("name[" + i + "]", pair.Value);
            }

            FormUrlEncodedContent content = new(values);

            HttpResponseMessage responseMessage = await httpClient.PostAsync("api/user-rankings", content);
            string response = await responseMessage.Content.ReadAsStringAsync();

            List<ScoreboardEntryModel>? playerData = JsonConvert.DeserializeObject<List<ScoreboardEntryModel>>(response);
            if (playerData == null)
            {
                await loggingService.LogAsync(new(LogSeverity.Error, "HTWUserService", "playerData is null"));
                return;
            }

            KeyValuePair<ulong, string>[] pairs = verifiedUsers.ToArray();
            for (int i = 0; i < verifiedUsers.Count; i++)
            {
                KeyValuePair<ulong, string> pair = pairs[i];
                if (!verifiedUsers.ContainsKey(pair.Key)) continue; //Falls sich der User ausgeloggt hat 

                SocketGuildUser? user = socketUsers.FirstOrDefault(socketUser => socketUser.Id == pair.Key);
                if (user == null)
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

                ScoreboardEntryModel? scoreboardEntryModel = playerData.FirstOrDefault(x => x.Name == pair.Value);
                if (scoreboardEntryModel == null)
                {
                    continue;
                }

                await user.ModifyAsync(x => x.Nickname = $"{user.Username} #{scoreboardEntryModel.Rank}");
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
