using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;

namespace HTWDiscordBot.Services.HTW
{
    //Scoreboard Logik
    public class ScoreboardService
    {
        private readonly DiscordSocketClient client;
        private readonly HttpClientService httpService;
        private readonly HtmlParserService htmlParserService;
        private readonly LoggingService loggingService;

        public ScoreboardService(DiscordSocketClient client, HttpClientService httpService, HtmlParserService htmlParserService, LoggingService loggingService)
        {
            this.client = client;
            this.httpService = httpService;
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
            HttpRequestMessage requestMessage = new(HttpMethod.Get, "highscore");
            HttpResponseMessage responseMessage = await httpService.httpClient.SendAsync(requestMessage);

            return htmlParserService.ParseScoreBoard(await responseMessage.Content.ReadAsStringAsync());
        }

        //Gibt den Scoreboard Eintrag eines Spielers zurück
        public async Task<Embed?> GetPlayerdataAsync(string username)
        {
            HttpRequestMessage requestMessage = new(HttpMethod.Get, "highscore");
            HttpResponseMessage responseMessage = await httpService.httpClient.SendAsync(requestMessage);

            string playerData = "**" + String.Format("{0,-6} {1,-9} {2,-40}", "Platz", "Punktzahl", "Benutzername").Replace(" ", "᲼") + "**";

            List<HtmlNode>? scoreboardEntry = htmlParserService.GetScoreBoardEntry(await responseMessage.Content.ReadAsStringAsync(), username);

            if (scoreboardEntry != null)
            {
                playerData += String.Format("\n{0,-5} {1,-9} {2,-40}", scoreboardEntry[0].InnerText, scoreboardEntry[2].InnerText, scoreboardEntry[1].InnerText).Replace(" ", "᲼");

                return CreateScoreboardEmbed(playerData);
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