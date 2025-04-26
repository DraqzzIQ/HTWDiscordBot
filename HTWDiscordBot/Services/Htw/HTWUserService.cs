using Discord;
using Discord.WebSocket;
using HtwDiscordBot.Models;
using Newtonsoft.Json;

namespace HtwDiscordBot.Services.Htw;

public class HtwUserService(
    IHttpClientFactory httpClientFactory,
    DiscordSocketClient client,
    ConfigService configService,
    ScoreboardService scoreboardService,
    LoggingService loggingService)
{
    private readonly ScoreboardService scoreboardService = scoreboardService;

    // key is the discord user id, value is the htw username
    private readonly string path = "verifiedUsers.json";
    private Dictionary<ulong, string> verifiedUsers = new();

    public async Task InitializeAsync()
    {
        verifiedUsers = await ReadDictionaryAsync();
    }

    // check if user login is valid
    public async Task<string> IsRealUserAsync(string username, string token, ulong id)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("client");

        HttpRequestMessage requestMessage = new(HttpMethod.Get, $"verify/{username}/{token}");
        HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);

        string? response = await responseMessage.Content.ReadAsStringAsync();

        if (response == "not valid")
            return "Login war nicht erfolgreich. Bitte überprüfe Benutzername und Token und versuche es erneut";

        verifiedUsers[id] = username;

        await WriteDictionaryAsync(verifiedUsers);
        await UpdateNicknames();
        return $"Login erfolgreich, {username}";
    }

    public async Task LogoutAsync(ulong id)
    {
        if (!verifiedUsers.ContainsKey(id))
            return;

        SocketGuild guild = client.GetGuild(configService.Config.ServerID);
        SocketGuildUser? user = guild.GetUser(id);

        await user.ModifyAsync(x => x.Nickname = user.GlobalName);

        verifiedUsers.Remove(id);
        await WriteDictionaryAsync(verifiedUsers);
    }

    // update the nicknames of all verified users
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

        List<ScoreboardEntryModel>?
            playerData = JsonConvert.DeserializeObject<List<ScoreboardEntryModel>>(response);
        if (playerData == null)
        {
            await loggingService.LogAsync(new(LogSeverity.Error, "HtwUserService", "playerData is null"));
            return;
        }

        KeyValuePair<ulong, string>[] pairs = verifiedUsers.ToArray();
        for (int i = 0; i < verifiedUsers.Count; i++)
        {
            KeyValuePair<ulong, string> pair = pairs[i];
            if (!verifiedUsers.ContainsKey(pair.Key)) continue; // user was removed in the meantime

            SocketGuildUser? user = socketUsers.FirstOrDefault(socketUser => socketUser.Id == pair.Key);
            if (user == null)
            {
                verifiedUsers.Remove(pair.Key);
                await WriteDictionaryAsync(verifiedUsers);
                continue;
            }

            // check if bot has enough permissions to change the nickname
            if (user.Hierarchy >= guild.GetUser(client.CurrentUser.Id).Hierarchy)
            {
                await user.Guild.GetTextChannel(configService.Config.ChallengeChannelID).SendMessageAsync(
                    $"{MentionUtils.MentionUser(pair.Key)} Ich habe nicht genug Rechte um deinen Nicknamen zu ändern :(");
                verifiedUsers.Remove(pair.Key);
                await WriteDictionaryAsync(verifiedUsers);
                continue;
            }

            ScoreboardEntryModel? scoreboardEntryModel = playerData.FirstOrDefault(x => x.Name == pair.Value);
            if (scoreboardEntryModel == null)
            {
                continue;
            }

            int usernameLength = 32 - scoreboardEntryModel.Rank.ToString().Length + 2;
            await user.ModifyAsync(x => x.Nickname = $"{scoreboardEntryModel.Name[..Math.Min(usernameLength, scoreboardEntryModel.Name.Length)]} #{scoreboardEntryModel.Rank}");
            await Task.Delay(1000); // wait 1 second to not get rate limited
        }
    }

    private async Task WriteDictionaryAsync(Dictionary<ulong, string> dict)
    {
        await File.WriteAllTextAsync(path, JsonConvert.SerializeObject(dict, formatting: Formatting.Indented));
    }

    private async Task<Dictionary<ulong, string>> ReadDictionaryAsync()
    {
        if (!File.Exists(path))
        {
            await WriteDictionaryAsync(new Dictionary<ulong, string>());
        }

        Dictionary<ulong, string>? dict =
            JsonConvert.DeserializeObject<Dictionary<ulong, string>>(await File.ReadAllTextAsync(path));

        return dict ?? new Dictionary<ulong, string>();
    }
}