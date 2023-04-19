using Discord;
using Discord.WebSocket;

namespace HTWDiscordBot.Services
{
    //Regestriert Slash Commands
    internal class SlashCommandService
    {
        private readonly DiscordService discordService;
        private readonly HTWService htwService;
        private readonly HtmlParserService htmlParserService;

        public SlashCommandService(DiscordService discordService, HTWService htwService)
        {
            this.discordService = discordService;
            this.htwService = htwService;
            this.htmlParserService = htmlParserService;
        }

        public Task InitializeAsync()
        {
            discordService.client.SlashCommandExecuted += SlashCommandHandler;
            return Task.CompletedTask;
        }

        //Wird ausgeführt, wenn ein Slash Command ausgeführt wird
        private async Task SlashCommandHandler(SocketSlashCommand command)
        {
            switch (command.Data.Name)
            {
                case "start":
                    await HandleStartCommandAsync(command);
                    break;
                case "stop":
                    await HandleStopCommandAsync(command);
                    break;
                case "scoreboard":
                    await HandleScoreboardCommandAsync(command);
                    break;
                case "playerdata":
                    await HandlePlayerDataCommandAsync(command);
                    break;
            }
        }

        //Startet die Überprüfung auf neue Aufgaben
        private async Task HandleStartCommandAsync(SocketSlashCommand command)
        {
            await command.RespondAsync("Started");
            await htwService.SetShouldCheckAsync(true);
        }

        //Stoppt die Überprüfung auf neue Aufgaben
        private async Task HandleStopCommandAsync(SocketSlashCommand command)
        {
            await command.RespondAsync("Stopped");
            await htwService.SetShouldCheckAsync(false);
        }

        //Gibt das Scoreboard aus
        private async Task HandleScoreboardCommandAsync(SocketSlashCommand command)
        {
            Embed scoreboard = new EmbedBuilder()
                .WithTitle("Scoreboard")
                .WithColor(Color.Blue)
                .WithDescription(await htwService.GetScoreboardAsync())
                .WithCurrentTimestamp().Build();

            await command.RespondAsync(embed: scoreboard);
        }

        private async Task HandlePlayerDataCommandAsync(SocketSlashCommand command)
        {
            Embed playerData = new EmbedBuilder()
                    .WithTitle("Player-Daten")
                    .WithColor(Color.Blue)
                    .WithDescription(await htwService.GetPlayerDataAsync(command.Data.Options.First()))
                    .WithCurrentTimestamp().Build();

            await command.RespondAsync(embed: playerData);
        }
    }
}
