using Discord;
using Discord.WebSocket;
using HTWDiscordBot.Models;
using Newtonsoft.Json;

namespace HTWDiscordBot.Services.HTW
{
    //Scoreboard Logik
    public class ScoreboardService
    {
        private readonly DiscordSocketClient client;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly LoggingService loggingService;

        public List<ScoreboardEntryModel>? Scoreboard { get; private set; }

        public ScoreboardService(DiscordSocketClient client, IHttpClientFactory httpClientFactory, LoggingService loggingService)
        {
            this.client = client;
            this.httpClientFactory = httpClientFactory;
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

            Scoreboard = await GetScoreboardAsync();

            IMessage? message = textChannel.GetMessagesAsync(1).FlattenAsync().Result.FirstOrDefault();


            if (message == null || message.Author.Id != client.CurrentUser.Id)
                await textChannel.SendMessageAsync(embed: CreateScoreboardEmbed(await CreateScoreboardAsync(Scoreboard.Take(50))));
            else
                await textChannel.ModifyMessageAsync(message.Id, async m => m.Embed = CreateScoreboardEmbed(await CreateScoreboardAsync(Scoreboard.Take(50))));
        }

        //Gibt den Scoreboard Eintrag eines Spielers zurück oder null, wenn der Spieler nicht im Scoreboard ist
        public async Task<Embed?> GetPlayerdataAsync(string username)
        {
            ScoreboardEntryModel? scoreboardEntry = await GetScoreboardEntryAsync(username);

            if (scoreboardEntry == null)
                return null;

            return CreateScoreboardEmbed(await CreateScoreboardAsync(new ScoreboardEntryModel[] { scoreboardEntry }));
        }

        //Sucht einen Scoreboard Eintrag anhand des Benutzernamens
        public async Task<ScoreboardEntryModel?> GetScoreboardEntryAsync(string username)
        {
            return Scoreboard?.Where(entry => entry.Name.Equals(username, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
        }



        //Parsed das Json Scoreboard von der HTW Seite
        private async Task<List<ScoreboardEntryModel>> GetScoreboardAsync()
        {
            HttpClient httpClient = httpClientFactory.CreateClient("client");

            HttpRequestMessage requestMessage = new(HttpMethod.Get, "api/highscore");
            HttpResponseMessage responseMessage = await httpClient.SendAsync(requestMessage);

            return JsonConvert.DeserializeObject<List<ScoreboardEntryModel>>(await responseMessage.Content.ReadAsStringAsync()) ?? new List<ScoreboardEntryModel>();
        }

        //Erstellt ein Scoreboard als String
        private async Task<string> CreateScoreboardAsync(IEnumerable<ScoreboardEntryModel> scoreboard)
        {
            string codeblock = $"```ansi\n\u001b[1;32mPlatz  Punktzahl Benutzername\u001b[0m\n";

            foreach (ScoreboardEntryModel entry in scoreboard)
            {
                codeblock += $"{entry.Rank.ToString().PadRight(2)}\t {entry.Score}\t  {entry.Name}\n";
            }

            return codeblock += "```";
        }

        //Erstellt einen Discord Embed mit dem Scoreboard
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