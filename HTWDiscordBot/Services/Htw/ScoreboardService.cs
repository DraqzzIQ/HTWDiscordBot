using Discord;
using Discord.WebSocket;
using HtwDiscordBot.Models;
using Newtonsoft.Json;

namespace HtwDiscordBot.Services.Htw;

public class ScoreboardService(
    DiscordSocketClient client,
    IHttpClientFactory httpClientFactory,
    LoggingService loggingService)
{
    private List<ScoreboardEntryModel>? Scoreboard { get; set; }

    public async Task UpdateScoreboardAsync(ulong textChannelID)
    {
        SocketTextChannel? textChannel = await client.GetChannelAsync(textChannelID) as SocketTextChannel;

        if (textChannel == null)
        {
            await loggingService.LogAsync(new(LogSeverity.Error, "ScoreboardService",
                "TextChannel konnte nicht gefunden werden!"));
            return;
        }

        Scoreboard = await GetScoreboardAsync();

        IMessage? message = textChannel.GetMessagesAsync(1).FlattenAsync().Result.FirstOrDefault();


        if (message == null || message.Author.Id != client.CurrentUser.Id)
            await textChannel.SendMessageAsync(
                embed: CreateScoreboardEmbed(await CreateScoreboardAsync(Scoreboard.Take(50))));
        else
            await textChannel.ModifyMessageAsync(message.Id,
                async m => m.Embed = CreateScoreboardEmbed(await CreateScoreboardAsync(Scoreboard.Take(50))));
    }

    // returns scoreboard entry of player or null if player is not in the scoreboard
    public async Task<Embed?> GetPlayerdataAsync(string username)
    {
        HttpClient httpClient = httpClientFactory.CreateClient("client");

        Dictionary<string, string> values = new()
        {
            { "name[0]", username }
        };

        FormUrlEncodedContent content = new(values);

        HttpResponseMessage responseMessage = await httpClient.PostAsync("api/user-rankings", content);
        string response = await responseMessage.Content.ReadAsStringAsync();

        List<ScoreboardEntryModel>?
            playerData = JsonConvert.DeserializeObject<List<ScoreboardEntryModel>>(response);
        if (playerData == null || playerData.Count < 1)
        {
            await loggingService.LogAsync(new(LogSeverity.Error, "ScoreboardService", "playerData is null"));
            return null;
        }

        return CreateScoreboardEmbed(await CreateScoreboardAsync(new ScoreboardEntryModel[] { playerData[0] }));
    }


    private async Task<List<ScoreboardEntryModel>> GetScoreboardAsync()
    {
        loggingService.Log(new(LogSeverity.Info, "ChallengeService", "Requesting api/top100"));
        HttpClient httpClient = httpClientFactory.CreateClient("client");

        HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/top100");
        HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);

        return JsonConvert.DeserializeObject<List<ScoreboardEntryModel>>(await responseMessage.Content
                   .ReadAsStringAsync()) ??
               new List<ScoreboardEntryModel>();
    }

    private async Task<string> CreateScoreboardAsync(IEnumerable<ScoreboardEntryModel> scoreboard)
    {
        string codeblock = $"```ansi\n\u001b[1;32mPlatz  Punktzahl Benutzername\u001b[0m\n";

        foreach (ScoreboardEntryModel entry in scoreboard)
        {
            codeblock += $"{entry.Rank.ToString(),-2}\t {entry.Score}\t  {entry.Name}\n";
        }

        return codeblock += "```";
    }

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