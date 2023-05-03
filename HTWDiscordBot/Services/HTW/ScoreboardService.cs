using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;

namespace HTWDiscordBot.Services.HTW
{
    //Scoreboard Logik
    public class ScoreboardService
    {
        private readonly DiscordSocketClient client;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly HtmlParserService htmlParserService;
        private readonly LoggingService loggingService;

        public ScoreboardService(DiscordSocketClient client, IHttpClientFactory httpClientFactory, HtmlParserService htmlParserService, LoggingService loggingService)
        {
            this.client = client;
            this.httpClientFactory = httpClientFactory;
            this.htmlParserService = htmlParserService;
            this.loggingService = loggingService;
        }

        //Updated das Scoreboard
        public async Task UpdateScoreboardAsync(ulong textChannelID)
        {
            SocketTextChannel? textChannel = await client.GetChannelAsync(textChannelID) as SocketTextChannel;

            if (textChannel == null)
            {
                await loggingService.LogAsync(new(LogSeverity.Error, "ScoreboardService", "TextChannel konnte nicht gefunden werden!"));
                return;
            }

            string scoreboard = await GetScoreboardAsync();

            IMessage? message = textChannel.GetMessagesAsync(1).FlattenAsync().Result.FirstOrDefault();

            if (message == null || message.Author.Id != client.CurrentUser.Id)
                await textChannel.SendMessageAsync(embed: CreateScoreboardEmbed(scoreboard));
            else
                await textChannel.ModifyMessageAsync(message.Id, m => m.Embed = CreateScoreboardEmbed(scoreboard));

        }

        //Gibt ein Scoreboard als String zurück
        private async Task<string> GetScoreboardAsync()
        {
            HttpClient httpClient = httpClientFactory.CreateClient("client");

            HttpRequestMessage requestMessage = new(HttpMethod.Get, "highscore");
            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);

            return htmlParserService.ParseScoreBoard(await responseMessage.Content.ReadAsStringAsync());
        }

        //Gibt den Scoreboard Eintrag eines Spielers zurück
        public async Task<Embed?> GetPlayerdataAsync(string username)
        {
            HttpClient httpClient = httpClientFactory.CreateClient("client");

            HttpRequestMessage requestMessage = new(HttpMethod.Get, "highscore");
            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);

            string codeblock = $"```ansi\n\u001b[1;32mPlatz  Punktzahl Benutzername\u001b[0m\n";

            List<HtmlNode>? scoreboardEntry = htmlParserService.GetScoreBoardEntry(await responseMessage.Content.ReadAsStringAsync(), username);

            if (scoreboardEntry != null)
            {
                codeblock += $"{scoreboardEntry[0].InnerText.PadRight(2)}\t {scoreboardEntry[2].InnerText}\t  {scoreboardEntry[1].InnerText}\n";
                codeblock += "```";
                return CreateScoreboardEmbed(codeblock);
            }
            else
                return null;
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